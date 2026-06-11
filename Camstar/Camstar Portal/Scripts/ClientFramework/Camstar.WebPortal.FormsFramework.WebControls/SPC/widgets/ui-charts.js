//Note: should not be referenced directly in HTML file. Will be used by   bundler.js
// The idea behid is to merge all these files in one file named dist/ui-chart.js. That file will be used in demo or 3rd party app. 
'use strict';
(function ($) {
    /**
     * @typedef {string} scaling
     * @description type of yAxis Scaling
     * @property {string} controlLimit yAxis will be scaled according to the minimum and maximum value of the charts Control limits
     * @property {string} toleranceLimit yAxis will be scaled according to the minimum and maximum value of the charts Tolerance limits
     * @property {string} default The min and max value of the yAxis will be automatically calculated based on the min/max value of the whole chart
    **/
    var scaling = {
        controlLimit:'controlLimit',
        toleranceLimit: 'toleranceLimit',
        default:'default'
    }


    /**
      * @typedef {string} chartTypes
      * @description Type of the control chart for variable characteristics. This can be used for different chart types parameter.
      * @property {string} xb_s - xb/s chart
      * @property {string} mxb_ms - mxb_ms chart
      * @property {string} xb_R - xb_R chart
      * @property {string} mxb_mR - mxb_mR chart
      * @property {string} x_ms - x_ms chart
      * @property {string} x_mR - x_mR chart
      * @property {string} xb - xb chart
      * @property {string} s - s chart
      * @property {string} r - range chart
      * @property {string} hist_S - Histogram chart where the array histogramClassesByS is used {@link OuputModel} only valid in Histogram Function {@link HistoChart}
      * @property {string} hist_T -Histogram chart where the array histogramClassesByTolerances is used {@link OuputModel} only valid in Histogram Function {@link HistoChart}
      * @property {string} svc - Single Value Chart
      * @property {string} med -Median chart
      * @property {string} med_R med_R chart
    **/
    var chartTypes = {
        xb_s: 1,
        mxb_ms: 2,
        xb_R: 3,
        mxb_mR: 4,
        x_ms: 5,
        x_mR: 6,
        xb: 7,
        s: 8,
        R: 9,
        hist_S: 10,
        //Histogram by Tolerance
        hist_T: 11,
        svc: 12,
        ms: 13,
        mR: 14,
        med: 15,
        med_R: 16,
        cu_Sum: 17,
        mxb: 18,
        x: 19
    };
    var movingMeanWeightings = {
        Uniformly: 1,
        Exponentially: 2
    };
    var attrChartTypes = {
        p: 7,
        np: 8,
        C: 9,
        U: 10
    };
    var siemensColors = {
        SiemensSnow: "#FFFFFF",
        PLBlack4: '#1E1E1E',
        SIEMENS_NATURAL_BLUE_DARK: "#006487",
        SIEMENS_BLUE_DARK: "#005F87",
        SIEMENS_STATUS_RED_DARK: "#DC0000",
        SIEMENS_STATUS_GREEN_DARK: "#0A9B00",
        SIEMENS_STATUS_GREEN_LIGHT:"#28E632",
        SiemensYellowDark: "#FFB900",
        SiemensBlue9: "#0F789B",
        SiemensBlueLight: "#50BED7",
        PL_BLACK_22: '#D2D2D2',
        SiemensBlue13: "#3296B9",
        PL_BLACK_19: '#B4B4B4',
        violationZone: 'red',
        //colors for the Stacked Pareto(UX Guidlines 30 Colors)
        //http://uxhub.net.plm.eds.com/docs/web_framework_ux_reference/data_visualization/default_coloring/
        SiemensYellow: '#e5c04c',
        SiemensRed: '#db535a',
        SiemensGray: '#606a75',
        SiemensOrange: '#e2894d',
        SiemensPurple: '#7f4664',
        SiemensGreen: '#689962',
        SiemensPink: '#e8aab8',
        SimenesBrown: '#a58f6f',
        SiemensTurkies: '#7fb2ac',
        SiemensDarkBlue: '#00557d',
        SiemensDarkYellow: '#a8852d',
        SiemensDarkRed: '#993140',
        SiemensDarkGray: '#32393f',
        SiemensDarkOrange: '#aa6130',
        SiemensDarkPurple: '#512d43',
        SiemensDarkGreen: '#476642',
        SiemensDarkPink: '#ad687f',
        SimenesDarkBrown: '#705e4b',
        SiemensDarkTurkies: '#4d7c73',
        SiemensLightBlue: '#46aac1',
        SiemensLightYellow: '#ead096',
        SiemensLightRed: '#eaacb9',
        SiemensLightGray: '#aab4bc',
        SiemensLightOrange: '#efc2a3',
        SiemensLightPurple: '#ba83a5',
        SiemensLightGreen: '#a5c69e',
        SiemensLightPink: '#efcbd9',
        SimenesLightBrown: '#ccbca8',
        SiemensLightTurkies: '#b4d6d0'
    };
    var chartSeriesColors = {
        Histogram: {
            Bin: siemensColors.SiemensBlue13,
            ToleranceLimit: siemensColors.SIEMENS_STATUS_RED_DARK,
            Xbb: siemensColors.SIEMENS_STATUS_GREEN_DARK,
            StandardDeviation: siemensColors.SIEMENS_NATURAL_BLUE_DARK
        },
        ControlChart: {
            ControlLimit: siemensColors.SIEMENS_STATUS_RED_DARK,
            WarningLimit: siemensColors.SiemensYellowDark,
            ToleranceLimit: siemensColors.SIEMENS_STATUS_RED_DARK,
            xbarProcess: siemensColors.SIEMENS_STATUS_GREEN_DARK,
            ProcessViolation: siemensColors.SIEMENS_STATUS_RED_DARK,
            xbarX: siemensColors.SiemensBlue9,
            Sr: siemensColors.SiemensBlue9,
            Marker: siemensColors.SIEMENS_STATUS_RED_DARK,
            InfoMarker: siemensColors.SiemensBlueLight,
            YellowMarker: siemensColors.SiemensYellowDark,
            FirstChoice: siemensColors.SIEMENS_STATUS_GREEN_DARK,
            SecondChoice: siemensColors.SIEMENS_STATUS_GREEN_LIGHT,
            ThirdChoice: siemensColors.SiemensYellowDark,
            CenterOfTolerances: siemensColors.PLBlack4,
            averageOfAllValues: siemensColors.SIEMENS_NATURAL_BLUE_DARK,
            calculatedMax: siemensColors.SiemensLightYellow,
            calculatedMin: siemensColors.SiemensLightYellow,
            oneThirdOfControlLimits: "#FF0F00",
            twoThirdsOfControlLimits: "#FF0FFF",
            nominalValue: siemensColors.SiemensGreen,
        },
        SingleValueChart: {
            ControlLimit: siemensColors.SIEMENS_STATUS_RED_DARK,
            WarningLimit: siemensColors.SiemensYellowDark,
            ToleranceLimit: siemensColors.SIEMENS_STATUS_RED_DARK,
            Measurement: siemensColors.SiemensBlue9,
            xbp: siemensColors.SIEMENS_STATUS_GREEN_DARK,
            Marker: siemensColors.SIEMENS_STATUS_RED_DARK,
            YellowMarker: siemensColors.SiemensYellowDark,
            FirstChoice: siemensColors.SIEMENS_STATUS_GREEN_DARK,
            SecondChoice: siemensColors.SIEMENS_STATUS_GREEN_LIGHT,
            ThirdChoice: siemensColors.SiemensYellowDark,
            CenterOfTolerances: siemensColors.PLBlack4,
            averageOfAllValues: siemensColors.SIEMENS_NATURAL_BLUE_DARK,
            oneThirdOfControlLimits: "#FF0F00",
            twoThirdsOfControlLimits: "#FF0FFF",
            nominalValue: siemensColors.SiemensGreen,
            additionalMeasurement: siemensColors.SiemensGray,
            lowerMiddlethird: "#FF0F00",
            upperMiddlethird: "#FF0FFF"
        },
        AttrControlChart: {
            ControlLimit: siemensColors.SIEMENS_STATUS_RED_DARK,
            WarningLimit: siemensColors.SiemensYellowDark,
            ToleranceLimit: siemensColors.SIEMENS_STATUS_RED_DARK,
            main: siemensColors.SiemensBlue9,
            Marker: siemensColors.SIEMENS_STATUS_RED_DARK,
            ProcessMeanValue: siemensColors.SIEMENS_STATUS_GREEN_DARK,
            localMeanValue: siemensColors.SIEMENS_STATUS_GREEN_LIGHT
        },
        ProbabilityPlot:
        {
            mainSeries: siemensColors.SiemensBlue9,
            slope: siemensColors.PLBlack4,
            ToleranceLimit: siemensColors.SIEMENS_STATUS_RED_DARK,
            StandardDeviation: siemensColors.SIEMENS_NATURAL_BLUE_DARK
        },
        Pareto:
        {
            DefectsCount: siemensColors.SIEMENS_BLUE_DARK,
            DefectsCounthover: siemensColors.SIEMENS_NATURAL_BLUE_DARK
        },
        CUSumChart:
        {
            CUSumLow: siemensColors.SiemensBlueLight,
            CUSumHigh: siemensColors.SIEMENS_BLUE_DARK,
            CUSumControlLimit: siemensColors.SIEMENS_STATUS_RED_DARK,
            CUSumMeanValue: siemensColors.SIEMENS_STATUS_GREEN_DARK
        },
        CUCountChart: {
            controlLimit: siemensColors.SIEMENS_STATUS_RED_DARK,
            meanValue: siemensColors.SIEMENS_STATUS_GREEN_DARK,
            mainSeries: siemensColors.SiemensBlue9
        },
        MultiVisDefect: {
            firstVisual: siemensColors.SiemensBlue9,
            secondVisual: siemensColors.SiemensYellow,
            thirdVisual: siemensColors.SiemensRed,
            fourthVisual: siemensColors.SiemensGray,
            fivethVisual: siemensColors.SiemensOrange,
            sixthVisual: siemensColors.SiemensPurple,
            seventhVisual: siemensColors.SiemensGreen,
            eightVisual: siemensColors.SiemensPink,
            ninethVisual: siemensColors.SimenesBrown,
            tenthVisual: siemensColors.SiemensTurkies,
            eleventhVisual: siemensColors.SiemensDarkBlue,
            twelvethVisual: siemensColors.SiemensDarkYellow,
            thirtennthVisual: siemensColors.SiemensDarkRed,
            fourtennthVisual: siemensColors.SiemensDarkGray,
            fivetennthVisual: siemensColors.SiemensDarkOrange,
            sixtennthVisual: siemensColors.SiemensDarkPurple,
            seventennthVisual: siemensColors.SiemensDarkGreen,
            eightennthVisual: siemensColors.SiemensDarkPink,
            ninetennthVisual: siemensColors.SimenesDarkBrown,
            twentiethVisual: siemensColors.SiemensDarkTurkies,
            twentyfirstVisual: siemensColors.SiemensLightBlue,
            twentysecondVisual: siemensColors.SiemensLightYellow,
            twentythirdVisual: siemensColors.SiemensLightRed,
            twentyfourthVisual: siemensColors.SiemensLightGray,
            twentyfivethVisual: siemensColors.SiemensLightOrange,
            twentysixthVisual: siemensColors.SiemensLightPurple,
            twentyseventhVisual: siemensColors.SiemensLightGreen,
            twentyeightVisual: siemensColors.SiemensLightPink,
            twentyninethVisual: siemensColors.SimenesLightBrown,
            twentytenthVisual: siemensColors.SiemensLightTurkies
        },
        multiVarControlChart: {
            firstVisual: siemensColors.SiemensBlue9,
            secondVisual: siemensColors.SiemensYellow,
            thirdVisual: siemensColors.SiemensGreen,
            fourthVisual: siemensColors.SiemensGray,
            fivethVisual: siemensColors.SiemensOrange,
            sixthVisual: siemensColors.SiemensPurple,
            seventhVisual: siemensColors.SiemensRed,
            eightVisual: siemensColors.SiemensPink,
            ninethVisual: siemensColors.SimenesBrown,
            tenthVisual: siemensColors.SiemensTurkies,
            eleventhVisual: siemensColors.SiemensDarkBlue,
            twelvethVisual: siemensColors.SiemensDarkYellow,
            thirtennthVisual: siemensColors.SiemensDarkRed,
            fourtennthVisual: siemensColors.SiemensDarkGray,
            fivetennthVisual: siemensColors.SiemensDarkOrange,
            sixtennthVisual: siemensColors.SiemensDarkPurple,
            seventennthVisual: siemensColors.SiemensDarkGreen,
            eightennthVisual: siemensColors.SiemensDarkPink,
            ninetennthVisual: siemensColors.SimenesDarkBrown,
            twentiethVisual: siemensColors.SiemensDarkTurkies,
            twentyfirstVisual: siemensColors.SiemensLightBlue,
            twentysecondVisual: siemensColors.SiemensLightYellow,
            twentythirdVisual: siemensColors.SiemensLightRed,
            twentyfourthVisual: siemensColors.SiemensLightGray,
            twentyfivethVisual: siemensColors.SiemensLightOrange,
            twentysixthVisual: siemensColors.SiemensLightPurple,
            twentyseventhVisual: siemensColors.SiemensLightGreen,
            twentyeightVisual: siemensColors.SiemensLightPink,
            twentyninethVisual: siemensColors.SimenesLightBrown,
            twentytenthVisual: siemensColors.SiemensLightTurkies,
            ControlLimit: siemensColors.SIEMENS_STATUS_RED_DARK,
            WarningLimit: siemensColors.SiemensYellowDark,
            ToleranceLimit: siemensColors.SIEMENS_STATUS_RED_DARK,
        }
    };
    var titleStyle = {
        fontSize: '15.5pt',
        color: siemensColors.PLBlack4
    };
    var axiesStyle = {
        fontSize: '7.5pt',
        color: siemensColors.PLBlack4
    };
    var axiesTitleStyle = {
        fontSize: '12.5pt',
        color: siemensColors.PLBlack4
    };
    var siemensTooltip = {
        // enabled: false
        split: false, // to show seperate each tooltip e.g. Series, UWL etc.
        shared: false,
        headerFormat: '',
        formatter: undefined,
        backgroundColor: siemensColors.SiemensSnow,
        borderWidth: 0,
        padding: 0,
        VerticalAlignValue: 'middle',
        useHTML: true,
        distance:5
    };

    /**
     * The IDs of the series used in control chart. These can be used in addPoint, updatePoint methods or to hide series.
     * @typedef {string} seriesIds
     * @memberof ControlChart
     * @property {string} ucl_1 - Upper control limit id of the first series.
     * @property {string} lcl_1 - Lower control limit id of the first series.
     * @property {string} uwl_1 - Upper warn limit id of the first series.
     * @property {string} lwl_1 - Lower warn limit id of the first series.
     * @property {string} xbp_1 - Process mean value id of the first series.
     * @property {string} series__1 - XB series id of the first series.
     * 
     * @property {string} firstChoiceUpperLimit - First Choice upper border series id.
     * @property {string} firstChoiceLowerLimit - First Choice lower border series id.
     * @property {string} secondChoiceUpperLimit - Second Choice upper border series id.
     * @property {string} secondChoiceUpperLimit - Second Choice lower border series id.
     * @property {string} thirdChoiceUpperLimit - Third Choice upper border series id.
     * @property {string} thirdChoiceUpperLimit - Third Choice lower border series id.
     * @property {string} centerOfTolerances - center of tolerances series id.
     * @property {string} averageOfAllValues - average of all measurements series id.
     * @property {string} oneThirdOfUpperControlLimit - one third of upper control limits series id.
     * @property {string} twoThirdsOfUpperControlLimit - two thirds of upper control limits series id.
     * @property {string} oneThirdOfLowerControlLimit - one third of lower control limits series id.
     * @property {string} twoThirdsOfLowerControlLimit - two thirds of lower control limits series id.
     * @property {string} calculatedMax - max value of the supgroup series id.
     * @property {string} calculatedMin - min value of the supgroup series id.
     * 
     * @property {string} ucl_2 - Upper control limit id of the second series.
     * @property {string} lcl_2 - Lower control limit id of the second series.
     * @property {string} uwl_2 - Upper warn limit id of the second series.
     * @property {string} lwl_2 - Lower warn limit id of the second series.
     * @property {string} xbp_2 - Process mean value id of the second series.
     * @property {string} series__2 - Series id of the second main series. It can be s, r, etc. series.
     * @property {string} boxplot - The boxplot series id.
     * @property {string} SPC_ZONE_A - The SPC zone A id.
     * @property {string} SPC_ZONE_B1 - The SPC zone B1,C1 id.
     * @property {string} SPC_ZONE_B2 - The SPC zone B2,C2 id.
     * @property {string} measurement - The measurment series id.
     * @property {string} utl_1 - The upper tolerance limit id.
     * @property {string} ltl_1 - The upper tolerance limit id.
     * @example
     * <caption> The series Id can be use like this add a point in a specific series </caption>
     * $('#chart').chart('addPoint', 'ucl_1', 19.90);
     * 
     * // to hide a series in chart , the series can be used too
     *  * $('#chart').chart(
     *      {
     *          locale: "en",
     *          data: {
     *              specifications: {
     *                              subgroupSize : 2,
     *                              controlChartType : 'xb_s',     
     *          },
     *          subgroups: [],  // Array of objects in defined format
     *      },
     *      decimalPlaces: 2,
     *      hiddenSeries: [
     *                      {
     *                          "seriesId" : "lcl",
     *                          "showInChart": true,
     *                          "showInLegend": false
     *                      }
     *                  ],
     *      onSeriesClick: function (data) {
     *          console.log(data);
     *      },
     *      onLegendClick : function (data) {
     *          console.log(data);
     *      }
     *      });
     */

    /**
     * The IDs of the series used in single value chart.These can be used to hide specific series. 
     * @typedef {string} seriesIds
     * @memberof SingleValueChart
     * @property {string} ucl - Upper control limit series id.
     * @property {string} lcl - Lower control limit series id.
     * @property {string} uwl - Upper warn limit series id.
     * @property {string} lwl - Lower warn limit series id.
     * @property {string} xbp - Process mean value series id.
     * @property {string} utl - Upper control limit series id.
     * @property {string} ltl - Lower control limit series id.
     * 
     * @property {string} firstChoiceUpperLimit - First Choice upper border series id.
     * @property {string} firstChoiceLowerLimit - First Choice lower border series id.
     * @property {string} secondChoiceUpperLimit - Second Choice upper border series id.
     * @property {string} secondChoiceUpperLimit - Second Choice lower border series id.
     * @property {string} thirdChoiceUpperLimit - Third Choice upper border series id.
     * @property {string} thirdChoiceUpperLimit - Third Choice lower border series id.
     * @property {string} centerOfTolerances - center of tolerances series id.
     * @property {string} averageOfAllValues - average of all measurements series id.
     * @property {string} oneThirdOfUpperControlLimit - one third of upper control limits series id.
     * @property {string} twoThirdsOfUpperControlLimit - two thirds of upper control limits series id.
     * @property {string} oneThirdOfLowerControlLimit - one third of lower control limits series id.
     * @property {string} twoThirdsOfLowerControlLimit - two thirds of lower control limits series id.
     * @property {string} measurement__1 - Measurement series id.
     * @example 
     * <caption> The series Id can be use like this to hide the series </caption>
     * $('#svc').singleValueChart(
     *      {
     *          locale: "en",
     *          data: {
     *          specifications: {
     *                          subgroupSize : 2,
     *                          controlChartType : 'xb_s'
     *          },
     *          measurements : [], // Array of objects in defined format
     *          subgroups: [],  // Array of objects in defined format
     *      },
     *      decimalPlaces: 2,
     *      hiddenSeries: [
     *                      { 
     *                          "seriesId" : "lcl", 
     *                          showInChart: true,
     *                          showInLegend: false
     *                      }
     *                  ],
     *      onSeriesClick: function (data) {
     *          console.log(data);
     *      }, 
     *      onLegendClick : function (data) {
     *          console.log(data);
     *      }
     *      });
     * */

    var seriesIds = {
        singleValueChart: {
            ucl: 'ucl',
            lcl: 'lcl',
            uwl: 'uwl',
            lwl: 'lwl',
            utl: 'utl',
            ltl: 'ltl',
            xbp: 'xbp',
            measurement: 'measurements__1',
            flagSeries: 'flagSeries',
            firstChoiceUpperLimit: 'firstChoiceUpperLimit', 
            firstChoiceLowerLimit: 'firstChoiceLowerLimit',
            secondChoiceUpperLimit: 'secondChoiceUpperLimit',
            secondChoiceLowerLimit: 'secondChoiceLowerLimit',
            thirdChoiceUpperLimit: 'thirdChoiceUpperLimit',
            thirdChoiceLowerLimit: 'thirdChoiceLowerLimit',
            centerOfTolerances: 'centerOfTolerances',
            averageOfAllValues: 'averageOfAllValues',
            oneThirdOfUpperControlLimit: 'oneThirdOfUpperControlLimit',
            twoThirdsOfUpperControlLimit: 'twoThirdsOfUpperControlLimit',
            oneThirdOfLowerControlLimit: 'oneThirdOfLowerControlLimit',
            twoThirdsOfLowerControlLimit: 'twoThirdsOfLowerControlLimit',
            nominalValue: 'nominalValue',
            additionalMeasurement: 'additionalMeasurement',
            lowerMiddlethird: 'lowerMiddlethird',
            upperMiddlethird: 'upperMiddlethird'
        },
        HistogramChart: {
            Classes: 'classes'
        },
        controlChart: {
            ucl1: 'ucl_1',
            lcl1: 'lcl_1',
            uwl1: 'uwl_1',
            lwl1: 'lwl_1',
            xbp1: 'xbp_1',
            series1: 'series__1',

            ucl2: 'ucl_2',
            lcl2: 'lcl_2',
            uwl2: 'uwl_2',
            lwl2: 'lwl_2',
            xbp2: 'xbp_2',
            series2: 'series__2',

            utl1: 'utl_1',
            ltl1: 'ltl_1',

            boxplot: 'boxplot', // this is for custom icon for boxplot
            boxplot_main: 'boxplot_main', // shouldn't be documented 
            toolChanged: 'tool_changed',
            measurement: 'measurement',
            flagSeries: 'flagSeries',
            SPCZoneA: 'SPC_ZONE_A',
            SPCZoneB1: 'SPC_ZONE_B1',
            SPCZoneB2: 'SPC_ZONE_B2',
            SPCZoneC1: 'SPC_ZONE_C1',
            SPCZoneC2: 'SPC_ZONE_C2',

            firstChoiceUpperLimit: 'firstChoiceUpperLimit',
            firstChoiceLowerLimit: 'firstChoiceLowerLimit',
            secondChoiceUpperLimit: 'secondChoiceUpperLimit',
            secondChoiceLowerLimit: 'secondChoiceLowerLimit',
            thirdChoiceUpperLimit: 'thirdChoiceUpperLimit',
            thirdChoiceLowerLimit: 'thirdChoiceLowerLimit',
            centerOfTolerances: 'centerOfTolerances',
            averageOfAllValues: 'averageOfAllValues',
            oneThirdOfUpperControlLimit: 'oneThirdOfUpperControlLimit',
            twoThirdsOfUpperControlLimit: 'twoThirdsOfUpperControlLimit',
            oneThirdOfLowerControlLimit: 'oneThirdOfLowerControlLimit',
            twoThirdsOfLowerControlLimit: 'twoThirdsOfLowerControlLimit',
            calculatedMax: 'calculatedMax',
            calculatedMin: 'calculatedMin',
            nominalValue:'nominalValue'
        },
        visControlChart: {
            nonConformanceRate: 'nonConformanceRate',
            numberOfDefects: 'numberOfDefects',
            upperControlLimitOfControlChart1: 'ucl_vis'
        },
        cuSumChart: {
            cuSumLow: 'cuSumLow',
            cuSumHigh: 'cuSumHigh',
            cuSumUCL: 'cuSumUCL',
            cuSumLCL: 'cuSumLCL',
            cuSumCL: 'cuSumCL'
        },
        cuCountChart: {
            cuCount: 'cuCount',
            lcl: 'ucl',
            ucl: 'lcl',
            xbp: 'xbp'
        },
        AtributiveControlChart: {
            atributiveflagSeries: 'attrFlagSeries'
        }
    };
    var onLegendClickEvent =
    {
        seriesID: undefined,
        isVisible: true
    };
    var onSeriesClickEvent =
    {
        subgroupNumber: undefined,
        seriesName: undefined,
        point: undefined,
        seriesID: undefined
    };
    var svgRepository =
    {
        ToolChanged: '<svg id="ToolChanged" width="16" height="16" data-name="Livello 1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><g id="g827"><path id="path9" d="M2.165,21.943a2.548,2.548,0,0,1-1.283-.725A2.485,2.485,0,0,1,.173,20a1.826,1.826,0,0,1,.036-1.2c.051-.1,1.815-2.084,3.92-4.412s3.822-4.25,3.816-4.268S7.192,9.34,6.286,8.419L4.641,6.745l-.949.948-.95.948L1.371,7.251,0,5.861l.926-.9c.509-.5,1.832-1.8,2.94-2.89L5.879.082l.2.142a1.466,1.466,0,0,0,.905.328A1.625,1.625,0,0,0,8,.2L8.285,0l.791.81.791.81-1.224,1.2-1.224,1.2,1.588,1.61L10.6,7.231l.1-.088a3.831,3.831,0,0,0,.272-.3l.174-.208-.072-.367A10.514,10.514,0,0,1,11,4.021a4.139,4.139,0,0,1,.841-2.127A5.184,5.184,0,0,1,15.227.051c.576-.069.824-.033,1.078.154.318.234.408.351.43.562a.451.451,0,0,1-.066.323c-.047.073-.473.573-.945,1.111-.842.957-.865.987-1.006,1.369-.2.551-.443,1.267-.432,1.275s.326.282.713.619l.7.612.184-.069c.1-.038.453-.191.781-.34.553-.251.613-.29.834-.546a19.56,19.56,0,0,1,1.822-1.972c.193-.067.4.018.67.276.328.314.355.429.33,1.354A4.335,4.335,0,0,1,19.8,6.922,4.176,4.176,0,0,1,18.24,8.744a5.3,5.3,0,0,1-3.217.765h-.744l-.254.3-.334.386c-.076.089-.07.1.279.477a3.3,3.3,0,0,1,.342.4c-.01.006-.193.129-.41.271l-.395.259-.268-.308c-.148-.168-1.581-1.637-3.184-3.266l-3.047-3.1-.137-.142-.729.7-.729.7.747.768c.412.423,1.91,1.946,3.33,3.386,1.514,1.535,2.574,2.643,2.561,2.677a4.272,4.272,0,0,1-.258.395l-.235.338-.271-.268-.27-.268L7.422,17.422C5.446,19.738,3.8,21.66,3.76,21.692a2.209,2.209,0,0,1-.313.176,2.236,2.236,0,0,1-1.282.075M3.11,20.9c.061-.055,1.7-1.967,3.651-4.25l3.54-4.152-.809-.808-.808-.809-.162.166c-.089.092-1.794,1.974-3.79,4.183-3.578,3.961-3.628,4.02-3.628,4.2a1.66,1.66,0,0,0,.606,1.2c.4.33,1.167.475,1.4.266M13.293,9.11c.5-.589.525-.6,1.357-.584A4.928,4.928,0,0,0,17.8,7.837,3.823,3.823,0,0,0,19.2,5.68a4.667,4.667,0,0,0,.127-1.074c-.012-.013-.1.066-.189.175s-.426.485-.74.837l-.57.641-.949.428c-.521.236-1.016.442-1.1.459-.295.061-.441-.032-1.441-.915-.525-.464-.986-.89-1.021-.945-.172-.26-.107-.569.385-1.895l.275-.739.682-.773a6.862,6.862,0,0,0,.654-.8A3.862,3.862,0,0,0,12.326,3.03a3.881,3.881,0,0,0-.333,2.036A8.36,8.36,0,0,0,12.1,6.3c.125.641.107.691-.457,1.307l-.34.37.789.79c.434.433.8.787.811.786s.186-.2.387-.438M5.178,4.915,8.064,2.077,8.55,1.6l-.184-.184-.183-.184-.391.128a2.557,2.557,0,0,1-1.537.023l-.263-.094-.731.719C2.454,4.767,1.369,5.845,1.369,5.872c0,.043,1.341,1.413,1.378,1.407.017,0,1.111-1.066,2.431-2.364" class="aw-theme-iconOutline" fill="#464646"></path><path id="path824" d="M17.6,16.943s-.034-.214-.074-.469c-.066-.413-.072-.465-.054-.473s.307-.052.656-.105c.73-.111.708-.107.7-.122s-.075-.066-.158-.133a3.668,3.668,0,0,0-1.785-.846,2.119,2.119,0,0,0-.821.076,2.453,2.453,0,0,0-.509.265,3.189,3.189,0,0,0-.9,1.115c-.04.077-.075.142-.077.145s-.827-.431-.853-.449c0,0,.044-.074.257-.4a3.481,3.481,0,0,1,1.58-1.539,3.548,3.548,0,0,1,.8-.184,2.709,2.709,0,0,1,.693.013,5.155,5.155,0,0,1,2.084,1.014.733.733,0,0,0,.169.107c0-.008-.04-.289-.092-.625s-.092-.612-.089-.615.216-.038.473-.077l.467-.072.226,1.461c.124.8.224,1.463.222,1.465s-.644.1-1.427.221-1.439.222-1.458.226a.093.093,0,0,1-.036,0Z" class="aw-theme-iconOutline" fill="#464646"></path><path id="path3-5" d="M17.5,24a6.487,6.487,0,1,1,.026,0H17.5m0-12A5.5,5.5,0,1,0,23,17.5,5.5,5.5,0,0,0,17.5,12" class="aw-theme-iconOutline" fill="#464646"></path><path id="circle5-7" d="M17,19.251A1.75,1.75,0,1,1,15.251,17.5h0A1.75,1.75,0,0,1,17,19.251Z" class="aw-theme-iconOutline" fill="#464646"></path><path id="circle5-4" d="M19.762,17.508a1.76,1.76,0,1,0,.024,0Zm.012,1h.01a.75.75,0,0,1,.75.75h0a.75.75,0,1,1-.76-.75Z" class="aw-theme-iconOutline" fill="#464646"></path></g></svg>',
        Attachment: '<svg id="Attachment"  width="16" height="16" version="1.1"    xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px" viewBox="0 0 24 24" enable-background="new 0 0 24 24" xml:space="preserve" width="25" height="25"><path class="aw-theme-iconOutline" fill="#464646" d="M6.2,23c-1.6,0-3.2-0.6-4.4-1.8c-2.4-2.4-2.4-6.3,0-8.7l9.8-9.8l0.7,0.7l-9.8,9.8c-2,2-2,5.3,0,7.3	c2,2,5.3,2,7.3,0l12-12c1.5-1.5,1.5-3.8,0-5.3c-1.5-1.5-3.8-1.5-5.3,0L6.4,13.4c-0.4,0.4-0.7,1-0.7,1.6c0,0.6,0.2,1.2,0.7,1.6	c0.9,0.9,2.4,0.9,3.3,0l8-8l0.7,0.7l-8,8c-1.3,1.3-3.4,1.3-4.7,0c-0.6-0.6-1-1.5-1-2.4c0-0.9,0.3-1.7,1-2.4L15.8,2.5	c0.9-0.9,2.1-1.4,3.4-1.4s2.5,0.5,3.4,1.4s1.4,2.1,1.4,3.4s-0.5,2.5-1.4,3.4l-12,12C9.3,22.4,7.7,23,6.2,23z"></path></svg>',
        Remark: '<svg id="Remark" width="16" height="16" data-name="Layer 1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"> <g id="g963"> <path id="rect13-9-3" d="M1,1V17H9.884v-1H1.943V2H22.057V13.3H23V1Z" class="aw-theme-iconOutline" fill="#464646"></path> <path id="path15-5" d="M3.318,2.92l-.64.768,7.2,5.994a3.549,3.549,0,0,0,4.633-.059L21.3,3.68l-.66-.752L13.85,8.869a2.516,2.516,0,0,1-3.336.045Z" class="aw-theme-iconOutline" fill="#464646"></path> <path id="line17-6" d="M9.928,8.953,2.7,14.414l.6.8,7.23-5.459Z" class="aw-theme-iconOutline" fill="#464646"></path> <path id="line19-5" d="M14.387,8.969l-.567.8,4.368,3.466h1.567Z" class="aw-theme-iconOutline" fill="#464646"></path> <path id="path833" d="M12.027,13.891a.913.913,0,0,0-.839.684.231.231,0,0,0-.018.035L9.639,20.731a.5.5,0,0,0,.364.606.481.481,0,0,0,.12.015h.111a3.1,3.1,0,0,0,6.155-.244.729.729,0,0,1,.375-.154,1.485,1.485,0,0,1,.363.045,3.1,3.1,0,0,0,6.166.353h.086a.452.452,0,0,0,.121-.016.189.189,0,0,0,.033-.013.449.449,0,0,0,.135-.067l.018-.009a.494.494,0,0,0,.214-.395.485.485,0,0,0-.052-.217l-1.5-6.01v-.009c0-.009-.01-.016-.012-.024A.906.906,0,0,0,21.4,13.9c-.4.044-.886.38-.886,1.73a.5.5,0,1,0,1,0,1.783,1.783,0,0,1,.006-.2l.777,3.111a3.086,3.086,0,0,0-5.047,1.483,1.928,1.928,0,0,0-.478-.061,1.425,1.425,0,0,0-.471.084,3.087,3.087,0,0,0-5.08-1.484L12,15.415c0,.065.006.13.006.211a.5.5,0,1,0,1,0c0-1.353-.483-1.69-.887-1.735ZM13.2,18.737h.016a2.115,2.115,0,0,1,2.2,2.025c0,.03,0,.06,0,.09A2.131,2.131,0,1,1,13.2,18.737Zm6.946,0a2.116,2.116,0,0,1,2.2,2.025c0,.03,0,.06,0,.09a2.121,2.121,0,1,1-2.205-2.115Z" class="aw-theme-iconOutline" fill="#464646"></path> <path id="path827" d="M14.323,19.467a.5.5,0,0,1-.047.7L12.528,21.7a.5.5,0,0,1-.688-.726l.03-.026,1.748-1.528a.5.5,0,0,1,.7.053Z" class="aw-theme-iconOutline" fill="#464646"></path> <path id="path825" d="M21.217,19.467a.5.5,0,0,1-.046.7L19.422,21.7a.5.5,0,0,1-.829-.377.5.5,0,0,1,.171-.375l1.748-1.528a.5.5,0,0,1,.7.053Z" class="aw-theme-iconOutline" fill="#464646"></path> </g></svg>',
        config: '<svg id="config" width="24" height="24" style="position:absolute;right:15px;top:15px;cursor:pointer;" version="1.1" xmlns="http://www.w3.org/2000/svg" x="0" y="0" viewBox="0 0 64 64" xml:space="preserve"><path class="prefix__aw-theme-iconHomeOutline" fill="#464646" d="M50.6 32h-5.2l-1.9-5.3-5 2.4-3.6-3.6 2.4-5-5.3-2v-5.2l5.3-1.9-2.4-5 3.6-3.6 5 2.4L45.4 0h5.2l1.9 5.2 5-2.4 3.6 3.6-2.4 5 5.3 1.9v5.2l-5.3 1.9 2.4 5-3.6 3.6-5-2.4-1.9 5.4zm-3.8-2h2.3l1.5-4.4c.2-.5.5-.8.9-1 .4-.2.9-.2 1.3 0l4.2 2 1.7-1.7-2-4.2c-.2-.4-.2-.9 0-1.3s.5-.7.9-.9l4.3-1.5v-2.3l-4.3-1.5c-.4-.2-.8-.5-1-.9-.2-.4-.2-.9 0-1.3l2-4.2L57 5.2l-4.2 2c-.4.2-.9.2-1.3 0a2 2 0 0 1-.9-.9L49.2 2h-2.3l-1.5 4.3c-.1.4-.5.8-.9.9-.4.2-.9.2-1.3 0l-4.2-2-1.7 1.7 2 4.2c.2.4.2.9 0 1.3a2 2 0 0 1-.9.9L34 14.8v2.3l4.3 1.5c.4.2.8.5 1 .9.2.4.2.9 0 1.3l-2 4.2 1.7 1.7 4.2-2c.4-.2.9-.2 1.3 0s.8.5.9 1l1.4 4.3zm11.5-9.5zm-.3-.9zm.5-7.7zm-5.9-6.3c0 .1 0 0 0 0z"/><path class="prefix__aw-theme-iconHomeOutline" fill="#464646" d="M48 21c-2.8 0-5-2.2-5-5s2.2-5 5-5 5 2.2 5 5-2.2 5-5 5zm0-8c-1.7 0-3 1.3-3 3s1.3 3 3 3 3-1.3 3-3-1.3-3-3-3zM22 49c-3.9 0-7-3.1-7-7s3.1-7 7-7 7 3.1 7 7-3.1 7-7 7zm0-12c-2.8 0-5 2.2-5 5s2.2 5 5 5 5-2.2 5-5-2.2-5-5-5z"/><path class="prefix__aw-theme-iconHomeOutline" fill="#464646" d="m26.8 63.2-5.1-5.9-5.3 5.6-6-2.6.6-7.7-7.8.2-2.4-6.1 5.9-5.1L1 36.3l2.6-6 7.7.6-.2-7.7 6.1-2.4 5.1 5.8 5.3-5.7 6 2.6-.6 7.8 7.8-.2 2.4 6.1-5.9 5.1 5.6 5.3-2.6 6-7.7-.6.3 7.8-6.1 2.4zm-5.1-8c.6 0 1.1.2 1.4.7l4.3 5 3.4-1.3-.2-6.6c0-.6.2-1.1.6-1.4.4-.3.9-.5 1.4-.5l6.5.5 1.5-3.4-4.8-4.5c-.4-.3-.6-.8-.6-1.4 0-.5.2-1 .6-1.3l5-4.3-1.3-3.4-6.5.2a1.9 1.9 0 0 1-1.9-2l.5-6.5-3.4-1.5-4.5 4.8a2 2 0 0 1-1.4.6c-.5 0-1-.2-1.3-.6l-4.3-4.9-3.4 1.3.2 6.5c0 .5-.2 1-.6 1.3-.4.4-.9.5-1.4.5L5 32.5l-1.5 3.4 4.8 4.5c.4.4.6.8.6 1.4 0 .5-.2 1-.6 1.3l-5 4.3 1.3 3.4 6.5-.2a1.9 1.9 0 0 1 1.9 2l-.5 6.4 3.4 1.5 4.5-4.8c.3-.3.8-.5 1.3-.5zm15.5-12.8zm-.7-.8z"/></svg>'
    };
    var statusesCategory=
    {
        processviolation_1: 'processviolation_1',
        outlier: 'outlier',
        toolchanged: 'toolchanged',
        remark: 'remark',
        attachment:'attachment'


    };

    

    


/**
 * @description Provide methods for drawing control chart .
 *
 * @namespace ControlChart
 */

/**
 * @description A new control chart can be drawn using this method.
 * @memberof ControlChart
 * @function chart
 * @requires highstock.js
 * @requires jQuery.js
 * @param {ControlChart.options} options - The chart options parameter
 * @tutorial CreateControlCharts
 * @example
    $('#chart').chart({
                        chartType: data.controlChartType,
                        locale: locale,
                        data: data,
                        chartTitle: 'xb/s Chart',
                        xAxisTitle: 'Subgroups',
                        decimalPlaces: 4,
                        height: '60%',
                        width: 800,
                        onSeriesClick: function (data) {
                            console.log(data);
                        },
                        series1yAxisMinValue: 1.20,
                        series1yAxisMaxValue: 2.20,
                        series2yAxisMinValue: 0.30,
                        series2yAxisMaxValue: 0.50,
                        onLegendClick: function (data) {
                           console.log(data);
                        }
                    });
 */

/**
 * @memberof ControlChart
 * @typedef {object} options
 * @property {ControlChart.legendSettings} legendSettings - Settings for position, layout and visibility of the chart legend
 * @property {string}  [chartType=xb_s] - Enumeration to specifies which chart type should be used see {@link chartTypes}
 * @property {string}  [locale=en]  User locale to display label and violations in a specific language
 * @property {ControlChart.data} data  JSON Array with subgroup in a defined format {@link ControlChart.data}
 * @property {string}  [chartTitle] - The Chart title
 * @property {string}  [xAxisTitle] - The xAxis Title 
 * @property {boolean}  [xAxisCustomInfoLabel=false] - If true, the default subgroup number label for xAxis will be overriden. by the property of customInfo object. where xAxisLabel is true. For more information  please refer {@link ControlChart.customInfo}
 * @property {number}  [decimalPlaces=4] - The data of chart will be round based on number of decimal places.
 * @property {number}  [height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number}  [width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string}  [yAxisSeries1Title] - A String specifies the yAxisSeries1Title name.
 * @property {number}  [series1yAxisMinValue] - A number specifies the min value of the series1 yAxis value.
 * @property {number}  [series1yAxisMaxValue] - A number specifies the max value of the series1 yAxis value.
 * @property {string}  [yAxisSeries2Title] - A String specifies the yAxisSeries2Title name.
 * @property {number}  [series2yAxisMinValue] - A number specifies the min value of the series2 yAxis value.
 * @property {number}  [series2yAxisMaxValue] - A number specifies the max value of the series2 yAxis value.
 * @property {ControlChart.seriesVisibilityObject} [options.hiddenSeries] - An array of object which contain the series ID, visibility in the legend and the initial visibility state of the series. 
 *              Remember to add every additional line you want to see, because they are invisible by default.
 *              The IDs of series can be found here {@link ControlChart.seriesIds} and object format can be seen here {@link ControlChart.seriesVisibilityObject}
 * @property {boolean} [boxplot=false] - If true then box plot will be displayed on control chart. The <b>highcharts-more.js</b> will must be referenced for box plot chart.
 * @property {ControlChart.onLegendClick} [onLegendClick=null] -  A callback can be register which will be fired when the legend item belonging to the series is clicked.
 * @property {ControlChart.onSeriesClick} [onSeriesClick=null] - A callback can be register which will be fired when a point on a Series is clicked.
 * @property {ControlChart.onChartLoaded} [onChartLoaded=null] - A callback can be register which will be invoked after chart is data is loaded and it is going to render. 
 * @property {string} [backgroundColor='#FFFFFF'] - Refer to chart background color
 * @property {ControlChart.series} [series] - To customize the series related options.
 * @property {ControlChart.yAxisUnit} [yAxisUnit] - The unit of y Axis object.
 * @property {boolean} [showMeasurements = false] - This will display the measurements series in chart, if measurements series is provided along subgroups in data. This will only work for xb, med, mxb_ms and mxb_mR chart types.
 * @property {string} [meanValueSeries1Name] - Mean value series1 name.
 * @property {string} [meanValueSeries2Name] - Mean value series2 name.
 * @property {bool} [drawSPCZones] - If the value is true, the SPC zone will be drawn on the chart
 * @property {boolean} [splitTooltip = true] - By default, the tooltip will be shown for each series individually. By setting this to false, each series values will be shown by selecting an individual point.
 * @property {boolean} [enableTolerenceLimits=false] - Enable or disable tolerence limit for specific control chart e.g. xb_ xb_s etc but not for R,s, ms, mR
 * @property {ControlChart.yAxisScale}  [options.yAxisScale] -determine the min/max value of the yAxis. If the user provides values for min and max then minScaling and maxScaling does not affect the scaling either the user specifies numeric values for min and max or specifies the scaling behavior according to {@link scaling}
 * @property {boolean}  [animation=false] - Enable or disable the initial animation
 * @property {ControlChart.titleSettings} [options.titleSettings]  - Provides options to position & rotate the chart title
 * @property {ControlChart.navigator} [options.navigator]  - The navigator object is to define start and end of navigator range as well as the number of selected values
 * @property {ControlChart.fontSize} [fontSize]-Option to set Fontsize of label & title.
 * @property {number} [xAxisLabelsRotation] Rotation of the labels in degrees. When undefined, the engine will try to hide/autorotate lables to avoid overlapping.
 * @property {ControlChart.axis} [options.xAxis] Settings for font, fontsize and color of xAxis labels 
 * @property {ControlChart.axis} [options.yAxis] Settings for font, fontsize and color of yAxis labels 
 
 * @example
 * $('#chart').chart({
 *      chartType: data.controlChartType,
 *      locale: locale,
 *      data: data,
 *      chartTitle: 'XB/s Chart',
 *      xAxisTitle: 'Subgroups',
 *      decimalPlaces: 4,
 *      height: '60%',
 *      width: undefined,
 *      onSeriesClick: function (data) {
 *                              console.log(data);
 *                              },
 *      series1yAxisMinValue: 1.10,
 *      series1yAxisMaxValue: 2.20,
 *      series2yAxisMinValue: 0.005,
 *      series2yAxisMaxValue: 0.006,
 *      xAxisCustomInfoLabel: true,
 *      onLegendClick: function (data) {
 *                              console.log(data);
 *                               },
 *       boxplot: true,
 *       series: {
 *          color: {
 *           warningLimit: '#FFDFGE',
 *           controlLimit: 'green',
 *           toleranceLimit: '#FFDFGD',
 *           processMeanValue: 'black',
 *           series1: '#FF2233',
 *           series2 : '#435576'
 *         }
 *       },
 *       xAxis: {
 *         labels: {
 *           fontSize: '10pt',
 *           font: 'Arial',
 *           color: 'black'
 *         }
 *       },
 *       yAxis: {
 *         labels: {
 *           fontSize: '10pt',
 *           font: 'Arial',
 *           color: 'red'
 *         }
 *       }           
 *  });
 */

/**
 * @typedef {object} yAxisUnit
 * @memberof ControlChart
 * @description To specify the series yAxis unit related properities.
 * @property {string} [text=""] - To specify the series colors.
 * @property {number} [x=60] - x position of the text.
 * @property {number} [y=null] - y position of the text. 
 * @property {number|string} [fontSize=null] - The font size of the text. If as number will be given then unit will be in pixel. As string in points can be size given e.g. '12pt'
 * @example
 * $('#chart').chart({
 *      chartType: data.controlChartType,
 *      locale: locale,
 *      data: data,
 *      chartTitle: 'XB/s Chart',
 *      xAxisTitle: 'Subgroups',
 *      decimalPlaces: 4,
 *      height: '60%',
 *      width: undefined,
 *      additionalLines: {
 *             "secondChoiceUpperLimit": 20.02,
 *             "secondChoiceLowerLimit": 19.98
 *      },
 *      hiddenSeries: [
 *           {  
 *              "seriesId": "firstChoiceUpperLimit",
 *              "showInChart": false,
 *              "showInLegend": true
 *           },
 *           {
 *              "seriesId": "secondChoiceUpperLimit",
 *              "showInLegend": true,
 *              "showInChart": false
 *           }
 *      ],
 *      onSeriesClick: function (data) {
 *          console.log(data);
 *      },
 *       onNavigationChanged: function (data) {
                    console.log(data)
                },
 *      series1yAxisMinValue: 1.10,
 *      series1yAxisMaxValue: 2.20,
 *      series2yAxisMinValue: 0.005,
 *      series2yAxisMaxValue: 0.006,
 *      xAxisCustomInfoLabel: true,
 *      onLegendClick: function (data) {
 *                              console.log(data);
 *                               },
 *       boxplot: true,
 *       yAxisUnit: {
 *                  text: 'DW05Unit01',
 *                  x: 100,
 *                  y: -20,
 *                  fontSize: '20pt'
 *                  },
 *       series: {
 *          color: {
 *                  warningLimit: '#FFDFGE',
 *                  controlLimit: 'green',
 *                  toleranceLimit: '#FFDFGD',
 *                  processMeanValue: 'black',
 *                  series1: '#FF2233',
 *                  series2 : '#435576'
 *                  }
 *           }
 *  });
 */

/**
 * @typedef {object} axis
 * @memberof ControlChart
 * @description Option to set font, fontsize and color of the axis labels.
 * @property {string} [fontSize]- Fontsize for labels.
 * @property {string} [font]- Font for labels.
 * @property {string} [color]- Color for labels.
 * @example
 *  xAxis: {
 *      labels: {
 *          fontSize: '10pt',
 *          font: 'Arial',
 *          color: 'red'
 *      }
 *  },
 *  yAxis: {
 *      labels: {
 *          fontSize: '8pt',
 *          font: 'Arial',
 *          color: 'black'
 *      }
 *  }
 */

/**
 * @description the behaviour of series, wether to hide from legend and / or they are visible initialy.
 *      Default for all additional lines is invisible and not shown in legend, all others are shown in legend and are visible.
 * @typedef {object} seriesVisibilityObject
 * @memberof ControlChart
 * @property {string} [seriesId] - the seriesId can be seen here {@link ControlChart.seriesIds}
 * @property {boolean} [showInChart] - initial show this series or not
 * @property {boolean} [showInLegend] - display this series in the legend
 * @example 
 * $('#chart').chart({
 *      chartType: data.controlChartType,
 *      locale: locale,
 *      data: data,
 *      chartTitle: 'XB/s Chart',
 *      xAxisTitle: 'Subgroups',
 *      decimalPlaces: 4,
 *      height: '60%',
 *      width: undefined,
 *      additionalLines: {
 *             "secondChoiceUpperLimit": 20.02,
 *             "secondChoiceLowerLimit": 19.98
 *      },
 *      hiddenSeries: [
 *           {
 *              "seriesId": "lcl_1",
 *              "showInChart": false,
 *              "showInLegend": true
 *           },
 *           {
 *              "seriesId": "secondChoiceUpperLimit",
 *              "showInLegend": true,
 *              "showInChart": false
 *           }
 *      ]
 *  });
 */

/**
 * @typedef {object} series
 * @memberof ControlChart
 * @description To specify the series related options e.g. color of series.
 * @property {object} color - To specify the series colors.
 * @property {string} [color.series1="#0F789B"] - It refer to the hexdecimal color or color name of main series of first row. e.g. xb
 * @property {string} [color.series2="#0F789B"] - It refer to the hexdecimal color or color name of main series of second row. e.g. xb
 * @property {string} [color.controlLimit="#DC0000"] - It refer to the hexdecimal color or color name of both series control limits. e.g. upper control limit 1, upper control limit 2 etc.
 * @property {string} [color.warningLimit="#FFB900"] - It refer to the hexdecimal color or color name of warning limit of both series. e.g. upper warn limit 1, lower warn limit 1 etc.
 * @property {string} [color.processMeanValue="#0A9B00"] - It refer to the hexdecimal color or color name of process mean value. e.g. xbp1, xbp2 etc.
 * @property {array} tooltip - To specify order of series in tooltip. Moreover series can be show or hide in tooltip with this param
 * @property {boolean} tooltip.showInTooltip=true - To show or hide a series in tooltip
 * @property {string} tooltip.seriesId - The id of the series
 * @property {number} tooltip.order - The order in which a series should be displayed in tooltip. Starting from 1.
 * @example
 * $('#chart').chart({
 *      chartType: data.controlChartType,
 *      locale: locale,
 *      data: data,
 *      chartTitle: 'XB/s Chart',
 *      xAxisTitle: 'Subgroups',
 *      decimalPlaces: 4,
 *      height: '60%',
 *      width: undefined,
 *      onSeriesClick: function (data) {
 *                              console.log(data);
 *                              },
 *      series1yAxisMinValue: 1.10,
 *      series1yAxisMaxValue: 2.20,
 *      series2yAxisMinValue: 0.005,
 *      series2yAxisMaxValue: 0.006,
 *      xAxisCustomInfoLabel: true,
 *      onLegendClick: function (data) {
 *                              console.log(data);
 *                               },
 *       boxplot: true,
 *       series: {
 *          color: {
 *                  warningLimit: '#FFDFGE',
 *                  controlLimit: 'green',
 *                  toleranceLimit: '#FFDFGD',
 *                  processMeanValue: 'black',
 *                  series1: '#FF2233',
 *                  series2 : '#435576'
 *                  }
 *           },
 *           tooltip: [
 *                      {
 *                          seriesId: 'ltl',
 *                          order: 1,
 *                          showInTooltip: true
 *                      },
 *                      {
 *                          seriesId: 'lcl',
 *                          order: 2,
 *                          showInTooltip: true
 *                      },
 *                      {
 *                          seriesId: 'lwl',
 *                          order: 3,
 *                          showInTooltip: true
 *                      },
 *                      {
 *                          seriesId: 'xbp',
 *                          order: 4,
 *                          showInTooltip: false
 *                      },
 *                      {
 *                          seriesId: 'uwl',
 *                          order: 5,
 *                          showInTooltip: true
 *                      },
 *                      {
 *                          seriesId: 'ucl',
 *                          order: 6,
 *                          showInTooltip: true
 *                      },
 *                      {
 *                          seriesId: 'utl',
 *                          order: 7,
 *                          showInTooltip: true
 *                      },
 *                      {
 *                          seriesId: 'series__1',
 *                          order: 9,
 *                          showInTooltip: true
 *                      }
 *               ]
 *      }
 */

/**
 * @description The input data format for control chart.
 * @typedef {Object} data
 * @memberof ControlChart
 * @example
 * // If measurements need not to be display
 * {
 *      specifications: {
 *                          subgroupSize: 5,
 *                          controlChartType : 'xb_s'                      
 *                      },
 *      subgroups : [
 *                      {
 *                          subgroupNumber: 0,
 *                          calculatedS: 0.00228,
 *                          calculatedR: 0.00600,
 *                          calculatedXb: 19.9958,
 *                          calculatedMin: 19.993,
 *                          calculatedMax: 19.999,
 *                          statuses: null,
 *                          upperControlLimit1Abs: 20.035,
 *                          lowerControlLimit1Abs: 19.965,
 *                          upperWarnLimit1Abs: 20.035,
 *                          lowerWarnLimit1Abs: 19.975,
 *                          upperControlLimit2Abs: 0.021,
 *                          lowerControlLimit2Abs: 0.0005,
 *                          upperWarnLimit2Abs: 0.018,
 *                          lowerWarnLimit2Abs: 0.0015,
 *                          calculatedProcessMeanValue1: 20.00453,
 *                          calculatedProcessMeanValue2: 0.00835581,
 *                          processSigma: 0.00888916271350903
 *                  }
 *              ]
 *  }

 * --------------------------------------------------------------------------------------------------------
 * // If measurements need to be displayed in chart then following is the data format
 * {
 *      specifications: {
 *                          subgroupSize: 5,
 *                          controlChartType : 'xb'                      
 *                      },
 *      subgroups : [
 *                      {
 *                          subgroupNumber: 0,
 *                          calculatedS: 0.002280350857290605,
 *                          calculatedR: 0.006000000000000227,
 *                          calculatedXb: 19.9958,
 *                          calculatedMin: 19.993,
 *                          calculatedMax: 19.999,
 *                          statuses: null,
 *                          upperControlLimit1Abs: 20.035,
 *                          lowerControlLimit1Abs: 19.965,
 *                          upperWarnLimit1Abs: 20.035,
 *                          lowerWarnLimit1Abs: 19.975,
 *                          upperControlLimit2Abs: 0.021,
 *                          lowerControlLimit2Abs: 0.0005,
 *                          upperWarnLimit2Abs: 0.018,
 *                          lowerWarnLimit2Abs: 0.0015,
 *                          calculatedProcessMeanValue1: 20.0045318,
 *                          calculatedProcessMeanValue2: 0.008355812950698488,
 *                          processSigma: 0.00888916271350903
 *                      }
 *                  ],
 *      measurements : [
 *                      {
 *                          referenceID: "0",
 *                          measuredValue: 19.993,
 *                          transformedMeasuredValue: 1.3008779659885696,
 *                          valueTimestamp: "0001-01-01T00:00:00",
 *                          statuses: null,
 *                          sequenceID: 0,
 *                          violations: null,
 *                          subgroupNumber: 10 // new subgroup number
 *                      }
 *                  ]
 *  }
 *  
 *  --------------------------------------------------------------------------------------------------
 * // If additional lines need to be displayed in chart then following is the data format
 * // Remember to check seriesVisibilityObject to make additional lines visible and/or display them in the legende
 * {
 *      specifications: {
 *                          subgroupSize: 5,
 *                          controlChartType : 'xb'
 *                      },
 *      subgroups : [
 *                      {
 *                          subgroupNumber: 0,
 *                          calculatedS: 0.002280350857290605,
 *                          calculatedR: 0.006000000000000227,
 *                          calculatedXb: 19.9958,
 *                          calculatedMin: 19.993,
 *                          calculatedMax: 19.999,
 *                          statuses: null,
 *                          upperControlLimit1Abs: 20.035,
 *                          lowerControlLimit1Abs: 19.965,
 *                          upperWarnLimit1Abs: 20.035,
 *                          lowerWarnLimit1Abs: 19.975,
 *                          upperControlLimit2Abs: 0.021,
 *                          lowerControlLimit2Abs: 0.0005,
 *                          upperWarnLimit2Abs: 0.018,
 *                          lowerWarnLimit2Abs: 0.0015,
 *                          calculatedProcessMeanValue1: 20.0045318,
 *                          calculatedProcessMeanValue2: 0.008355812950698488,
 *                          processSigma: 0.00888916271350903
 *                      }
 *                  ],
 *      measurements : [
*                      {
*                          referenceID: "0",
*                          measuredValue: 19.993,
*                          transformedMeasuredValue: 1.3008779659885696,
*                          valueTimestamp: "0001-01-01T00:00:00",
*                          statuses: null,
*                          sequenceID: 0,
*                          violations: null,
*                          subgroupNumber: 10 // new subgroup number
*                      }
*                  ],
 *      AdditionalLines: {
*                          firstChoiceUpperLimit: 20.01,
*                          firstChoiceLowerLimit: 19.99,
*                          secondChoiceUpperLimit: 20.02,
*                          secondChoiceLowerLimit: 19.98,
*                          thirdChoiceUpperLimit: 20.04,
*                          thirdChoiceLowerLimit: 19.96
*                          },
*      charts: [
*         {
*          controlChart : {
*            hiddenSeries [ 
*              {
*               seriesId: firstChoiceUpperLimit,
*				showInChart: true,
*				showInLegend: true
*              },
*              {
*               seriesId: secondChoiceUpperLimit,
*  				showInChart: true,
*				showInLegend: true
*              }
*          ]
*        }
*  }
*/

/**
 * @description An array of objects with following properties can be injected against each subgroup.
 * @typedef {object} customInfo
 * @memberof ControlChart
 * @property {string} label - The label to be display in tooltip.
 * @property {string} value - The value to be used to show in tooltip and for xAxis labels.
 * @property {boolean} showInTooltip - if true, the label and value will also be showin in tooltip for a particular subgrup as 'label: value'
 * @property {boolean} xAxisLabel - If true then value of will be showin on xAxis labels instead of default labels. e.g. subgroup number
 * @property {boolean} isRemark - If true then the info on the Custome Info object will be treated as a Remark which will be shown in a Flag series over the series
 * @example
 * <caption>A customInfo can be injected like this, if a data is defined in the given format, please refer {@link ControlChart.data}</caption>

    data.subgroups[0].customInfo = [
        {
            label: 'Charge Number',
            value: 'CH001',
            showInTooltip: true,
            xAxisLabel: false
        },
        {
            label: 'Date',
            value: '05-01-2020',
            showInTooltip: true,
            xAxisLabel: true
        }
    ];
  // Note: If against a subgroup more than one objects have 'xAxisLabel' true then the value by default
  // of the first object will be taken. e.g. If in the given example both objects have xAxisLabel true
  // then in chart the value of first object on xAxis label (for subgroup 0) will be displayed.
  // Which is in this case 'CH001'
 */

/**
 * @description The yAxisScale object is to determine the min/max values for both yAxis. If the user provide values for Min and Max then MinScaling and MaxScaling does not affect the scaling either the user specifies numeric values for min and mix or specifies the scaling behavior according to {@link scaling}
 * @typedef {object} yAxisScale
 * @memberof ControlChart
 * @property {number} [min = undefined] - The minimum value of the y axis. If null or undefined the minimum value is calculated based on minScaling.
 * @property {number} [max = undefined] - The maximum value of the y axis. If null or undefined the maximum value is calculated based on maxScaling.
 * @property {scaling} [minScaling = scaling.default] - refer to {@link scaling}.
 * @property {scaling} [maxScaling = scaling.default] - refer to {@link scaling}.
 * @property {number} [secondMin = undefined] - The minimum value of the y axis of second chart. If null or undefined the minimum value is calculated based on secondMinScaling.
 * @property {number} [secondMax = undefined] - The maximum value of the y axis. If null or undefined the maximum value is calculated based on maxScaling.
 * @property {scaling} [secondMinScaling = scaling.default] - refer to {@link scaling}.
 * @property {scaling} [secondMaxScaling: = scaling.default] - refer to {@link scaling}.
 * @example
 * yAxisScale: {
 *   min: undefined,
 *   max: undefined,
 *   minScaling: "controlLimit",
 *   maxScaling: "controlLimit",
 *   secondMin: undefined,
 *   secondMax: undefined,
 *   secondMinScaling: "controlLimit",
 *   secondMaxScaling: "controlLimit"
 *   }
 */

/**
 * @description The titleSettings object is to determine the position and rotation of the chart title
 * @typedef {object} titleSettings
 * @memberof ControlChart
 * @property {string} [align = undefined] - The horizontal alignment of the title. Can be one of "left", "center" and "right".
 * @property {number} [rotate = undefined] - The rotation of the text in degrees. 0 is horizontal, 270 is vertical reading from bottom to top.
 * @property {number} [x = undefind] - The x position of the title relative to the alignment
 * @property {number} [y = undefind] - The y position of the title relative to the alignment
 * @example
 *titleSettings:{
 *  align: 'left',
 *  rotate: '90',
 *  x: '35',
 *  y:'207'
 *},
 */

/**
 * @description The navigator object is to define start and end of navigator range as well as the number of selected values
 * @typedef {object} navigator
 * @memberof ControlChart
 * @property {number} [start = undefined] - first value to be selected in the navigator when defined alone then the last n values would be selected.
 * @property {number} [end = undefined] - last value to be selected in the navigator when defined alone then the first n values would be selected
 * @property {number} [selected = 25] - the number of selected value in the navigator, if start and end are set this value has no effect.
 * @example
 * navigator: {
 *              start: 0,
 *              end: 5,
 *              selected: 5
 *           },
 */


/**
    * @description An array of objects with following properties can be injected against each measurements.
    * @typedef {object} tooltip
    * @memberof ControlChart
    * @property {boolean} [showInTooltip=true] - To show or hide a series in tooltip
    * @property {string} seriesId - The id of the series
    * @property {number} order - The order in which a series should be displayed in tooltip. Starting from 1.
    * @example
    *  $('#svc').chart(
    *      {
    *          locale: "en",
    *          data: {
    *               specifications :
    *                   {
    *                       subgroupSize : 2,
    *                       controlChartType : 'xb_s',
    *                   }
    *               measurements : [], // Array of objects in defined format
    *               subgroups: [],  // Array of objects in defined format
    *              },
    *           decimalPlaces: 2,
    *           xAxisCustomInfoLabel: true,
    *           series: {
    *               tooltip: [
    *                   {
    *                       seriesId: 'ltl',
    *                       order: 1,
    *                       showInTooltip: true
    *                   },
    *                   {
    *                       seriesId: 'lcl',
    *                       order: 2,
    *                       showInTooltip: true
    *                   },
    *                   {
    *                       seriesId: 'lwl',
    *                       order: 3,
    *                       showInTooltip: true
    *                   },
    *                   {
    *                       seriesId: 'xbp',
    *                       order: 4,
    *                       showInTooltip: false
    *                   },
    *                   {
    *                       seriesId: 'uwl',
    *                       order: 5,
    *                       showInTooltip: true
    *                   },
    *                   {
    *                       seriesId: 'ucl',
    *                       order: 6,
    *                       showInTooltip: true
    *                   },
    *                   {
    *                       seriesId: 'utl',
    *                       order: 7,
    *                       showInTooltip: true
    *                   },
    *                   {
    *                       seriesId: 'series__1',
    *                       order: 9,
    *                       showInTooltip: true
    *                   }
    *               ]
    *           }
    *      });
*/
/**
 * @typedef {object} fontSize
 * @memberof ControlChart
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */

$.fn.chart = function (options) {

    var settings = {},
        tooltipRow = '<tr series_id="{2}"><td style="text-align: left; padding: 0;">{0}</td> <td style="text-align: right; padding: 0;">{1}</td></tr>',
        tooltipViolation = '<tr series_id="{2}"><td style="text-align: left; padding: 0; color: red;">{0}</td> <td style="text-align: right; color: red; padding: 0; color: red;">{1}</td></tr>',
        tooltipHr = '<tr><td colspan="2;" style = "padding: 3px 4px 3px 4px;"><hr style="margin-top: 0; padding: 0; margin-bottom: 0;"/></td></tr>',
        tooltipTableStart = '<table style="border-spacing: 0px;"><tbody>',
        tooltipTableEnd = '</tbody></table>';

    if (options && typeof options === 'object') {
        var _options = setDefault(options); // extend the object and set default

        settings = $.extend({
            chartType: chartTypes.xb_s, // default XB Chart
            chartName: options.chartType, // save the chart name e.g. xb_s etc.
            locale: 'en',
            data: {}, // JSON Array with subgroup
            chartTitle: getLocalizedText(options.chartType + '_chart', _options.locale),
            xAxisTitle: getLocalizedText('x_axis_subgroups', _options.locale),
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            backgroundColor: undefined,
            yAxisScale: {
                min: undefined,
                max: undefined,
                minScaling: undefined,
                maxScaling: undefined
            },
            navigator: {
                start: undefined,
                end: undefined,
                selected: 25
            },
            xAxis: {
                labels: {
                    fontSize: '11px',
                    font: 'Arial',
                    color: 'black'
                }
            },
            yAxis: {
                labels: {
                    fontSize: '11px',
                    font: 'Arial',
                    color: 'black'
                }
            },

            /* Series 1 Settings */
            yAxisSeries1Title: getLocalizedText(_options.seriesNames[0], _options.locale),
            series1Name: getLocalizedText(_options.seriesNames[0], _options.locale) || 'Series1',
            series1yAxisMinValue: undefined,
            series1yAxisMaxValue: undefined,
            meanValueSeries1Name: undefined,
            drawSPCZones: false,

            /* Series 2 Settings */
            yAxisSeries2Title: getLocalizedText(_options.seriesNames[1], _options.locale),
            series2Name: getLocalizedText(_options.seriesNames[1], _options.locale) || 'Series2',
            series2yAxisMinValue: undefined,
            series2yAxisMaxValue: undefined,
            meanValueSeries2Name: undefined,
            isSeries2Visible: !(_options.chartType === chartTypes.xb || _options.chartType === chartTypes.s || _options.chartType === chartTypes.R || _options.chartType === chartTypes.mR || _options.chartType === chartTypes.ms || _options.chartType === chartTypes.med || _options.chartType === chartTypes.mxb || _options.chartType === chartTypes.x),
            hiddenSeries: [], //TODO: Clear with TM, wether is this right that additional lines can be enabled through hidden series object and not based on data?
            xAxisCustomInfoLabel: false,
            boxplot: false,
            showMeasurements: false,
            splitTooltip: true,
            yAxisUnit: getDefaultyAxisUnitObj(),

            /*Event Functions*/
            onLegendClick: undefined,
            onSeriesClick: undefined,
            onChartLoaded: undefined,
            onNavigationChanged: undefined,

            series: getDefaultSeriesSettings(), //TODO: hiddenSeries should be removed and series object should be extend to show or hide series in legend and in chart
            siemensTooltip: {},
            legendSettings: {
                enabled: undefined,
                align: undefined,
                verticalAlign: undefined,
                layout: undefined,
                x: undefined,
                y: undefined
            },
            titleSettings: undefined,
            enableTolerenceLimits: false,
            animation: false,
            xAxisLabelsRotation: undefined
        }, _options);
    } else if (typeof options === 'string') { // Method call
        settings = $(this).data('settings') || {};  // restore the settings object from prev settings
    } else if (!options)
        throw 'Not valid options or parameter to the "chart" extension passed';

    var $this = this,
        _args = arguments,
        chart = {
            init: function () {
                var containerId = $this.attr('id'),
                    input = {
                        series1: [], UCL1: [], LCL1: [], UWL1: [], LWL1: [], xbP1: [], UTL1: [], LTL1: [],
                        series2: [], LCL2: [], UCL2: [], LWL2: [], UWL2: [], xbP2: [],
                        flagSeries: [], customInfo: [], subgroups: [], boxplot: [], median: [], measurements: [], flagremarks: [], flagAttachment: [],
                        zoneA: [], zoneB1: [], zoneB2: [], zoneC1: [], zoneC2: [],
                        firstChoiceUpperLimit: [], firstChoiceLowerLimit: [], secondChoiceUpperLimit: [], secondChoiceLowerLimit: [], thirdChoiceUpperLimit: [], thirdChoiceLowerLimit: [],
                        centerOfTolerances: [], averageOfAllValues: [], oneThirdOfUpperControlLimit: [], twoThirdsOfUpperControlLimit: [], oneThirdOfLowerControlLimit: [], twoThirdsOfLowerControlLimit: [], calculatedMax: [], calculatedMin: [], nominalValue: []
                    };

                if (settings.data && settings.data.subgroups) {
                    prepareData(input);

                    settings.inputs = input;// for custominfo tooltip
                    var seriesData = prepareSeries(input),
                        _chart = Highcharts.stockChart(containerId, prepareChartOptions(seriesData, input));
                    //  chart.xAxis[0].setExtremes(75, 100);	// Initial select last 25 samples
                    $this.data('chartApi', _chart); // required to call methods e.g. addPoint
                    $this.data('settings', settings);
                } else
                    throw 'Invalid data format exception: Series data is not in defined format';


            },

            /**
             * @function addPoint
             * @description Add a new point to a specific series the series is specified by its seriesId. The control chart must be drawn before using {@link ControlChart.chart} method.
             * @param {string} seriesId The id of the series that being updated. For IDs of the series please refer {@link ControlChart.seriesIds}
             * @param {number} newPoint The value of the point that being added.
             * @memberof ControlChart
             * @example
             * $('#chart').chart('addPoint', 'ucl_1', 19.90);
             * // This will add a new point at the right most side with the value of 19.90 in upper control limit of first series.
             */
            addPoint: function (seriesId, newPoint) {
                var __chart = $this.data('chartApi');
                if (__chart) {
                    if (typeof seriesId === 'string') { // it is update by id of chart
                        if (__chart.get(seriesId)) {
                            __chart.get(seriesId).addPoint(newPoint, true);
                        }
                    }
                }
            },


            /**
             * @function setXAxisRange
             * @description Set the xAxis to a specific Range. The control chart must be drawn before using {@link ControlChart.chart} method.
             * @param {number} minValue The minimal Value of the Range to which the xAxis schould be set
             * @param {number} maxValue The maximal Value of the Range to which the xAxis schould be set
             * @memberof ControlChart
             * @example
             * $('#chart').chart('setXAxisRange', 0,10);
             * // This will set the xAxis Range from 0 to 10
             */
            setXAxisRange: function (minValue, maxValue) {
                var __chart = $this.data('chartApi');
                if (minValue == null) {
                    minValue = 0;
                }
                if (__chart) {
                    if (maxValue != null && (minValue > maxValue)) {
                        throw 'The passed min Value is higher than the passed max Value';
                    }
                    else {
                        __chart.xAxis[0].setExtremes(minValue, maxValue);
                    }
                }
            },

            /**
             * @function addSubgroup
             * @description Add a new subgroup on the right most side of control chart.
             * @param {object} newSubgroup - The complete new subgroup which is provided like before to control chart
             * @memberof ControlChart
             * @example
             * $('#chart').chart('addSubgroup', {
             *                          subgroupNumber: 2,
             *                          calculatedS: 0.0106,
             *                          calculatedR: 0.0270,
             *                          calculatedXb: 19.9754,
             *                          statuses:
             *                                [
             *                                  {category: "processViolation_2", viewedPoints: null, points: null, lowerPercentage: null, upperPercentage: null},
             *                                  {category: "middleThird", viewedPoints: null, points: null, lowerPercentage: null, upperPercentage: null},
             *                                  {category: "processViolation_1", viewedPoints: null, points: null, lowerPercentage: null, upperPercentage: null},
             *                                  {category: "ucL_1", viewedPoints: null, points: null, lowerPercentage: null, upperPercentage: null}
             *                                ],
             *                            upperControlLimit1Abs: 20.014,
             *                            lowerControlLimit1Abs: 19.985,
             *                            upperWarnLimit1Abs: null,
             *                            lowerWarnLimit1Abs: null,
             *                            upperControlLimit2Abs: 0.01715,
             *                            lowerControlLimit2Abs: 0.00202,
             *                            upperWarnLimit2Abs: null,
             *                            lowerWarnLimit2Abs: null,
             *                            calculatedProcessMeanValue1: 20.0045,
             *                            calculatedProcessMeanValue2: 0.00835,
             *                            boxPlot: [ 19.958, 19.9655, 19.979, 19.9835, 19.985],
             *                            customInfo: [
             *                                         {
             *                                            label: "Charge Number",
             *                                            value: "CH00304",
             *                                            showInTooltip: true,
             *                                            xAxisLabel: false
             *                                          },
             *                                          {
             *                                             label: "Date",
             *                                             value: "22.1.2020",
             *                                             showInTooltip: true,
             *                                             xAxisLabel: true
             *                                           }
             *                                   ]
             *       });
             */
            addSubgroup: function (newSubgroup) {
                addNewSubgroup(newSubgroup);
            },

            /**
             * @function updatePoint
             * @description this function update the value of a proper point, the point can be identified by its seriesID and sortNumber. The control chart must be drawn before using {@link ControlChart.chart} method.
             * @param {string} seriesId The id of the series that being updated. For IDs of the series please refer {@link ControlChart.seriesIds}
             * @param {number} sgNum The subgroup Number of the point that being updated.
             * @param {number} newValue the new Value of the Point that being updated.
             * @memberof ControlChart
             * @name updatePoint
             * @example
             * $('#chart').chart('updatePoint', 'ucl_1', 2,  19.90);
             * // This will update the value of third subgroup with the new given value 19.90. Please remember that subgroup number always start from 0.
            */
            updatePoint: function (seriesId, sgNum, newValue) {
                var __chart = $this.data('chartApi');

                if (typeof newValue !== 'number') throw 'The new provided value is not a valid number';

                if (__chart) {
                    if (typeof seriesId === 'string' && __chart.get(seriesId)) { // it is update by id of chart
                        if (__chart.get(seriesId).data[sgNum]) {
                            var data = __chart.get(seriesId).data[sgNum];
                            data.update(newValue);
                        }
                    }
                }
            },

            /**
             * @function updateColor
             * @description To update the color of the series and chart background.
             * @memberof ControlChart
             * @param {object} colorScheme - The new color scheme for series
             * @param {string} colorScheme.series1 - The hexadecimal code or color name of main first series e.g. xb.
             * @param {string} colorScheme.series2 - The hexadecimal code or color name the second main series e.g. Range, s Series.
             * @param {string} colorScheme.controlLimit - The hexadecimal code or color name of control limit
             * @param {string} colorScheme.warningLimit - The hexadecimal code or color name of warning limit.
             * @param {string} colorScheme.processMeanValue - The hexadecimal code or color name of process mean value.
             * @example
             *  $('#chart').chart('updateColor', {
             *                                      series1: '#FF35AF',
             *                                      series2: '#FF35DF',
             *                                      controlLimit: 'blue',
             *                                      warningLimit : '#FF3522',
             *                                      processMeanValue : 'white',
             *                                      backgroundColor: 'red'
             *                                    });
             */
            updateColor: function (colorScheme) {
                updateChartColor(colorScheme);
            },

            /**
             * @description - Change the visible state of measurements series.
             * @memberof ControlChart
             * @function showMeasurements
             * @param {boolean} state - True will display the measurements series and false will hide it.
             * @example
             * $('#chart').chart('showMeasurements', true);
             */
            showMeasurements: function (state) {
                var chartApi = $this.data('chartApi');
                if (typeof state === 'boolean' && displayMeasurements() && chartApi.get(seriesIds.controlChart.measurement)) {
                    chartApi.get(seriesIds.controlChart.measurement).update({ visible: state }, true);
                }
            },

            /**
             * @description - Update a specific subgroup
             * @memberof ControlChart
             * @function updateSubgroup
             * @param {number} sgNum - The SubGroupNumber which need to be updated
             * @param {object} subgroup - The new Subgroup object with or without customInfo object. For details please see the example below             
             * @example
             * // to leave a value unchanged do not include it in the object
             * $('#chart').chart('updateSubgroup', 79, {	
             *                                          lcl1:		19.98,
             *                                          lcl2:		0.01,
             *                                          lwl1:		19.991,
             *                                          lwl2:		0.02,
             *                                          series1:	19.981,
             *                                          series2:	0.15,
             *                                          ucl1:		20.02,
             *                                          ucl2:		0.2,
             *                                          uwl1:		20.02,
             *                                          uwl2:		0.2,
             *                                          xbP1:		19.998,
             *                                          xbP2:		0.1,
             *                                          custominfo: newCustomInfoObject // see customInfo section
             *                                          });
             * 
             */
            updateSubgroup: function (sgNum, subgroup) {
                updateSubgroup(roundFloat(sgNum), subgroup);
            },

            /**
             * @description - Tooltip can be switched between combined and individual tooltip
             * @memberof ControlChart
             * @function splitTooltip
             * @param {boolean} state - True will show the tooltip for each series individually and false will combine the tooltip.             
             * @example
             * $('#chart').chart('splitTooltip', true);
             *
             */
            splitTooltip: function (state) {
                var chartApi = $this.data('chartApi');
                if (state === true) {
                    // settings.siemensTooltip.formatter = displayTooltip
                    settings.siemensTooltip.split = false;
                    chartApi.update({ tooltip: settings.siemensTooltip });
                    settings.splitTooltip = true;
                } else if (state === false) {
                    // settings.siemensTooltip.formatter = displayTooltip
                    settings.siemensTooltip.split = true;
                    chartApi.update({ tooltip: settings.siemensTooltip });
                    settings.splitTooltip = false;
                }
            },

            /**
             * @description - Export the chart as image. Only support the compatible HTML5 browsers
             * @memberof ControlChart
             * @function exportChart
             * @param {object} options - An object with specified properties
             * @param  {string} options.type - The export type. possible values are 'image/png', 'image/jpeg', 'applicaton/pdf' and 'image/svg+xml'. Default 'image/png'
             * @param  {string} options.filename - The file name without extension
             * @param  {number} options.width - The width of the chart to be exported.
             * @param  {number} options.height - The height of the chart to be exported.
             * @requires exporting.js
             * @requires offline-exporting.js
             * @example
             * $('#chart').chart('exportChart', {
             *                                      height:800, 
             *                                      width:1600, 
             *                                      type : 'image/jpeg', 
             *                                      filename: 'myChart'
             *                                  });
             */
            exportChart: function (options) {
                var chartApi = $this.data('chartApi');
                exportAsImage(chartApi, options);
            },

            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof ControlChart
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.  
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered. 
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#chart').chart('createBase64Image', 
            *                                       {
            *                                           containerId : 'sImageBase64', 
            *                                           width : 1800, 
            *                                           height: 600, 
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }

        };
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2], _args[3]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function yAxisLabels() {
        var unit = settings.yAxisUnit && isNullOrUndefined(settings.yAxisUnit.text) ? '' : settings.yAxisUnit.text;
        var yAxis = [
            {
                //min: settings.series1yAxisMinValue,
                //max: settings.series1yAxisMaxValue,
                min: getYaxis1MinScaling(),
                max: getYaxis1MaxScaling(),
                opposite: false,
                lineWidth: 2,
                labels: {
                    // rotation: -90, // settings.xAxisLabelsRotation,
                    formatter: function () {
                        return roundFloat(this.value, settings.decimalPlaces);
                    },
                    style: {
                        color: settings.yAxis.labels.color,
                        fontSize: settings.yAxis.labels.fontSize
                    }
                },
                title: {
                    text: settings.yAxisSeries1Title,
                    style: axiesTitleStyle
                },
                lineColor: siemensColors.PL_BLACK_22
            }
        ];

        if (settings.isSeries2Visible) {
            yAxis[0].height = '52%';
            yAxis.push({
                //min: settings.series2yAxisMinValue,
                //max: settings.series2yAxisMaxValue,
                min: getYaxis2MinScaling(),
                max: getYaxis2MaxScaling(),

                labels: {
                    // align: 'right',
                    // x: -8
                    formatter: function () {
                        return roundFloat(this.value, settings.decimalPlaces);
                    },
                    style: {
                        color: settings.yAxis.labels.color,
                        fontSize: settings.yAxis.labels.fontSize
                    }
                },
                title: {
                    text: settings.yAxisSeries2Title,
                    style: axiesTitleStyle
                },
                opposite: false,
                top: '53%',
                height: '46%',
                offset: 0,
                lineWidth: 2,
                lineColor: siemensColors.PL_BLACK_22
            });
        }

        yAxis.push(
            {// to show units of chart
                opposite: false,
                title: {
                    reserveSpace: false,
                    text: unit,
                    align: 'high',
                    rotation: 0,
                    x: settings.yAxisUnit.x,
                    y: settings.yAxisUnit.y,
                    style: {
                        fontSize: settings.yAxisUnit.fontSize,
                        color: siemensColors.PLBlack4
                    }
                }
            });
        return yAxis;
    }

    /**
 * @description The callback format which will handle when the Navigation bar chnages.
 * @memberof ControlChart
 * @callback onNavigationChanged 
 * @example
 *   data = { Max: 5, Min: 1 }
 */
    function onNavigationChanged(e) {
        if (typeof settings.onNavigationChanged === 'function') { // if callback is defined

            var response = { Max: e.max, Min: e.min };
            settings.onNavigationChanged(response);
        }
    }

    /**
    * @description The callback format which will handle the series click event.
    * @memberof ControlChart
    * @callback onSeriesClick
    * @param {object} data - An object with the below mentioned properties will be handed back as parameter.
    * @param {number} data.subgroupNumber - The subgroup number.
    * @param {string} data.seriesName - The series name.
    * @param {number} data.point - The Y value of this point.
    * @param {string} data.seriesId - The id of the clicked series.
    * @param {string} data.referenceId - The reference id from corresponding first/left measurement.
    * @param {array} data.violations The violations if handled for this point in point.
    * @param {array} data.customInfo the customInfo object injected against a subgroup.     *
    * @example
    *   data = { subgroupNumber: 1, seriesName: xb, point: 1, seriesId: ucl__1, violations: ['Run Down'], customInfo : [], referenceId: '1' }
    */
    function onSeriesClick(e) {
        if (typeof settings.onSeriesClick === 'function') { // if callback is defined
            var subgroupNumber,
                point,
                seriesID,
                seriesName,
                subgroups = settings.inputs.subgroups,
                violations,
                referenceId,
                response;   // response object for callback

            if (e && e.point && e.point.index < subgroups.length) {
                subgroupNumber = e.point.index;
                point = e.point.y;
                if (e.point.options && e.point.options.violations) {
                    violations = e.point.options.violations;
                }
                if (settings.data.subgroups[e.point.index]) {
                    referenceId = settings.data.subgroups[e.point.index].referenceID;
                }
            }
            if (e.point.series && typeof e.point.y === 'number') {
                seriesName = e.point.series.name;
                seriesID = e.point.series.options.id;
                point = e.point.y;
            }

            response = { subgroupNumber: subgroupNumber, seriesName: seriesName, point: point, seriesId: seriesID, violations: violations, referenceId: referenceId };

            if (settings.inputs.customInfo[e.point.index])
                response.customInfo = settings.inputs.customInfo[e.point.index];

            settings.onSeriesClick(response);
        }
    }

    /**
    * @description The callback format which will handle the legend click event. The default action is to toggle the visibility of the series. This can be prevented by returning false or calling event.preventDefault().
    * @memberof ControlChart
    * @callback onLegendClick
    * @param {object} data - The object hold the information of clicked legend.
    * @param {string} data.seriesId - The series id of the clicked legend item.
    * @param {boolean} data.isVisible - Flag to represent the state of series that whether series was visible before click event or not.
    * @example
    * // When a callback is registered with options in single value chart options
    *  $('#chart').chart(
    *                  {
    *                      locale: "en",
    *                      data: {   
    *                          subgroups: [],  // Array of objects in defined format
    *                      },
    *                      decimalPlaces: 2,
    *                      onLegendClick : function (data) {
    *                                          console.log(data);
    *                                            //The output will be like this
    *                                            // seriesId: "lcl_1", isVisible: false
    *                                       }
    *                  }
    *              );
    *
    */
    function onLegendClick(e) {
        if (typeof settings.onLegendClick === 'function') {
            var seriesID,
                isVisible,
                output;

            seriesID = e.target.userOptions.id;
            isVisible = this.visible;
            output = { seriesId: seriesID, isVisible: isVisible };
            settings.onLegendClick(output);
        }
    }

    /**
    * @description The callback which is called when a chart is drawn. 
    * @memberof ControlChart
    * @callback onChartLoaded
    * @param {object} e - The event object with chart API object
    * @example
    * // When a callback is registered with options in single value chart options
    *  $('#chart').chart(
    *                  {
    *                      locale: "en",
    *                      data: {
    *                          subgroups: [],  // Array of objects in defined format
    *                      },
    *                      decimalPlaces: 2,
    *                      onChartLoaded : function (data) {
    *                                          console.log(data);
    *                                       }
    *                  }
    *              );
    */
    function onChartLoaded(e) {
        if (typeof settings.onChartLoaded === 'function') { // if callback is defined
            settings.onChartLoaded(e);
        }
    }
    // ControlChart prepareData(input)
    function prepareData(input) {
        var movingMeanWeightingType = undefined;

        if (settings.data && settings.data.subgroups) {

            if (settings.data.specifications) {
                movingMeanWeightingType = parseMovingMeanWeighting(settings.data.specifications.movingMeanWeighting);
            }

            $.each(settings.data.subgroups, function (index, item) {
                extractSubgroupData(item, index, input, movingMeanWeightingType);
            });
        }
        if (settings.data && settings.data.measurements && settings.showMeasurements === true) {
            prepareMeasurementsSeries(input);
        }
        if (settings.data && settings.data.additionalLines) {
            var count = 0;
            if (settings.showMeasurements) {
                count = settings.data.measurements.length;
            } else {
                count = settings.data.subgroups.length;
            }
            //averageOfAllValues is not part of subgroup and must be alwyes filled 
            for (var i = 0; i < count; i++) {
                input.averageOfAllValues.push(roundFloat(settings.data.additionalLines.averageOfAllValues, null, true));

            }
            //when at least one additional line series length is 0 this means the additional lines are not available in the 
            //subgroup so that we fill them from additionalline Object directly other wise the seriess would have been filled 
            //in during the supgroup extraction
            if (input.firstChoiceUpperLimit.length == 0) {
                for (var i = 0; i < count; i++) {
                    input.firstChoiceUpperLimit.push(roundFloat(settings.data.additionalLines.firstChoiceUpperLimit, null, true));
                    input.firstChoiceLowerLimit.push(roundFloat(settings.data.additionalLines.firstChoiceLowerLimit, null, true));
                    input.secondChoiceUpperLimit.push(roundFloat(settings.data.additionalLines.secondChoiceUpperLimit, null, true));
                    input.secondChoiceLowerLimit.push(roundFloat(settings.data.additionalLines.secondChoiceLowerLimit, null, true));
                    input.thirdChoiceUpperLimit.push(roundFloat(settings.data.additionalLines.thirdChoiceUpperLimit, null, true));
                    input.thirdChoiceLowerLimit.push(roundFloat(settings.data.additionalLines.thirdChoiceLowerLimit, null, true));
                    input.centerOfTolerances.push(roundFloat(settings.data.additionalLines.centerOfTolerances, null, true));
                    input.oneThirdOfUpperControlLimit.push(roundFloat(settings.data.additionalLines.oneThirdOfUpperControlLimit, null, true));
                    input.twoThirdsOfUpperControlLimit.push(roundFloat(settings.data.additionalLines.twoThirdsOfUpperControlLimit, null, true));
                    input.oneThirdOfLowerControlLimit.push(roundFloat(settings.data.additionalLines.oneThirdOfLowerControlLimit, null, true));
                    input.twoThirdsOfLowerControlLimit.push(roundFloat(settings.data.additionalLines.twoThirdsOfLowerControlLimit, null, true));
                }

            }

        }
    }

    function extractSubgroupData(subgroup, index, input, movingMeanWeightingType) {
        var output = calculateLimits(subgroup, movingMeanWeightingType),

            statuses = subgroup.statuses,
            data = getViolation(statuses),
            newstatuses = data.newstatuses,
            violations = data.violations,
            customInfo = subgroup.customInfo || [],
            subgroupNumber = subgroup.subgroupNumber >= index ? subgroup.subgroupNumber : index;

        if (settings.xAxisCustomInfoLabel === true) {    // for xAxis
            if (subgroup.customInfo) {
                input.subgroups.push(getCustomxAxisLabel(subgroup.customInfo));
            } else { input.subgroups.push(null); } //show empty, if no corresponding custominfo is given
        } else
            input.subgroups.push(subgroupNumber);

        addSeries1Data(newstatuses, input, output, violations, index);

        input.customInfo.push(customInfo); // for tooltip

        if (settings.boxplot === true) {
            if (!isNullOrUndefined(output.seriesVal1)) {
                input.boxplot.push(output.boxplot);
            } else {
                input.boxplot.push(null);
            }
        }
        fillAdditionalLinesFromSupgroups(subgroup, input, index);
        addSeries2Data(newstatuses, input, output, violations, index);
        buildSpcZones(input, subgroup, settings.data.result.processValues.sigmaEstimated);
    }

    function fillAdditionalLinesFromSupgroups(subgroup, input, index) {
        if (subgroup.additionalLines) {
            input.firstChoiceUpperLimit.push((roundFloat(subgroup.additionalLines.firstChoiceUpperLimit, null, true)));
            input.firstChoiceLowerLimit.push((roundFloat(subgroup.additionalLines.firstChoiceLowerLimit, null, true)));
            input.secondChoiceUpperLimit.push((roundFloat(subgroup.additionalLines.secondChoiceUpperLimit, null, true)));
            input.secondChoiceLowerLimit.push((roundFloat(subgroup.additionalLines.secondChoiceLowerLimit, null, true)));
            input.thirdChoiceUpperLimit.push((roundFloat(subgroup.additionalLines.thirdChoiceUpperLimit, null, true)));
            input.thirdChoiceLowerLimit.push((roundFloat(subgroup.additionalLines.thirdChoiceLowerLimit, null, true)));
            input.centerOfTolerances.push((roundFloat(subgroup.additionalLines.centerOfTolerances, null, true)));
            input.oneThirdOfUpperControlLimit.push((roundFloat(subgroup.additionalLines.oneThirdOfUpperControlLimit, null, true)));
            input.twoThirdsOfUpperControlLimit.push((roundFloat(subgroup.additionalLines.twoThirdsOfUpperControlLimit, null, true)));
            input.oneThirdOfLowerControlLimit.push((roundFloat(subgroup.additionalLines.oneThirdOfLowerControlLimit, null, true)));
            input.twoThirdsOfLowerControlLimit.push((roundFloat(subgroup.additionalLines.twoThirdsOfLowerControlLimit, null, true)));

        } else {
            input.firstChoiceUpperLimit.push(null);
            input.firstChoiceLowerLimit.push(null);
            input.secondChoiceUpperLimit.push(null);
            input.secondChoiceLowerLimit.push(null);
            input.thirdChoiceUpperLimit.push(null);
            input.thirdChoiceLowerLimit.push(null);
            input.centerOfTolerances.push(null);
            input.oneThirdOfUpperControlLimit.push(null);
            input.twoThirdsOfUpperControlLimit.push(null);
            input.oneThirdOfLowerControlLimit.push(null);
            input.twoThirdsOfLowerControlLimit.push(null);
        }
        if (subgroup.calculatedMax)
            input.calculatedMax.push(roundFloat(subgroup.calculatedMax, null, true));
        if (subgroup.calculatedMin)
            input.calculatedMin.push(roundFloat(subgroup.calculatedMin, null, true));
        if (subgroup.nominalValue)//ControlChart
            input.nominalValue.push(roundFloat(subgroup.nominalValue, null, true));

    }

    function buildSpcZones(input, subgroup, processSigmaSpecified) {
        if (input && subgroup &&
            subgroup.calculatedProcessMeanValue1 &&
            (subgroup.processSigma || processSigmaSpecified) &&
            settings.drawSPCZones === true &&
            isRelevantSpcZoneChartType()) {
            var mean = subgroup.calculatedProcessMeanValue1;
            var sigma;
            if (subgroup.processSigma) {
                sigma = subgroup.processSigma;
            }
            else {
                sigma = processSigmaSpecified;
            }

            input.zoneA.push([calculateMeanSigma(mean, -sigma, 1), calculateMeanSigma(mean, sigma, 1)]);
            input.zoneB1.push([calculateMeanSigma(mean, sigma, 1), calculateMeanSigma(mean, sigma, 2)]);
            input.zoneB2.push([calculateMeanSigma(mean, sigma, 2), calculateMeanSigma(mean, sigma, 3)]);
            input.zoneC1.push([calculateMeanSigma(mean, -sigma, 1), calculateMeanSigma(mean, -sigma, 2)]);
            input.zoneC2.push([calculateMeanSigma(mean, -sigma, 2), calculateMeanSigma(mean, -sigma, 3)]);
        } else {
            input.zoneA.push([null, null]);
            input.zoneB1.push([null, null]);
            input.zoneB2.push([null, null]);
            input.zoneC1.push([null, null]);
            input.zoneC2.push([null, null]);

        }
    }

    function calculateMeanSigma(mean, sigma, factor) {

        return (mean + (sigma * factor))

    }

    function getViolation(statuses) {
        var data = { newstatuses: [], violations: [] };

        if (statuses && statuses.length > 0 && typeof statuses === 'object') {
            statuses.forEach(function (s) {
                if (s.category) {
                    if (s.category.toLowerCase() !== 'processviolation_1' && s.category.toLowerCase() !== 'processviolation_2')
                        data.violations.push(getLocalizedText(s.category, settings.locale));

                    data.newstatuses.push(s.category.toLowerCase());
                }
            });
        }

        return data;
    }

    function addSeries1Data(statuses, input, output, violations, index) {
        var series1Val = output.seriesVal1;
        if (statuses && statuses.length > 0 && typeof statuses === 'object') {
            if (statuses.indexOf("processviolation_1") > -1) {// to draw red triangle
                input.series1.push({
                    marker: {
                        fillColor: chartSeriesColors.ControlChart.Marker,
                        lineWidth: 3,
                        lineColor: chartSeriesColors.ControlChart.Marker,
                        symbol: 'triangle'
                    },
                    name: 'violation' + index,
                    y: series1Val,
                    violations: violations  //to get in tooltip and in click event
                });
            } else if (statuses.indexOf("outlier") > -1) {
                input.series1.push({
                    marker: {
                        fillColor: chartSeriesColors.ControlChart.YellowMarker,
                        lineWidth: 3,
                        lineColor: chartSeriesColors.ControlChart.YellowMarker,
                        symbol: 'triangle'
                    },
                    name: 'outlier',
                    y: series1Val,
                    violations: violations
                });
            } else if (statuses.indexOf("eliminated") > -1) {
                input.series1.push({
                    marker: {
                        fillColor: chartSeriesColors.ControlChart.Marker,
                        lineWidth: 3,
                        lineColor: chartSeriesColors.ControlChart.Marker,
                        symbol: 'cross'
                    },
                    name: 'eliminated',
                    y: series1Val,
                    violations: violations
                });
            } else if (statuses.indexOf("eliminatedbycalculation") > -1) {
                input.series1.push({
                    marker: {
                        fillColor: chartSeriesColors.ControlChart.Marker,
                        lineWidth: 3,
                        lineColor: chartSeriesColors.ControlChart.Marker,
                        symbol: 'cross'
                    },
                    name: 'eliminatedbycalculation',
                    y: series1Val,
                    violations: violations
                });
            }
            else
                input.series1.push(series1Val);

            if (statuses.indexOf("toolchanged") > -1) {

                input.flagSeries.push({
                    y: series1Val,
                    x: index,
                    title: svgRepository.ToolChanged,
                    text: getLocalizedText('ToolChanged', settings.locale),
                });
            }
            if (statuses.indexOf("remark") > -1) {
                input.flagSeries.push({
                    y: series1Val,
                    x: index,
                    title: svgRepository.Remark,
                    text: getCustomRemarks(settings, index)
                });
            }
            if (statuses.indexOf("attachment") > -1) {
                input.flagSeries.push({
                    y: series1Val,
                    x: index,
                    title: svgRepository.Attachment,
                    text: getLocalizedText('Attachment', settings.locale),

                });
            }
        } else {
            input.series1.push(series1Val);
            if (statuses && typeof statuses !== 'object') // if statuses are given in wrong format e.g. as string but not as object
                console.error('Violations statuses are not valid array of object. It should be an array of objects with defined format. Please consult doco.');
        }

        input.UCL1.push(output.ucl1);
        input.LCL1.push(output.lcl1);
        input.UWL1.push(output.uwl1);
        input.LWL1.push(output.lwl1);
        input.xbP1.push(output.xbp1);
        if (settings.enableTolerenceLimits === true) {
            input.UTL1.push(output.utl1);
            input.LTL1.push(output.ltl1);
        }
    }

    function addSeries2Data(statuses, input, output, violations, index) {
        if (settings.isSeries2Visible) {
            if (statuses.indexOf("processviolation_2") > -1)
                input.series2.push({
                    marker: {
                        fillColor: chartSeriesColors.ControlChart.Marker,
                        lineWidth: 3,
                        lineColor: chartSeriesColors.ControlChart.Marker,
                        symbol: 'triangle'
                    },
                    name: 'violation' + index,
                    y: output.seriesVal2,
                    violations: violations
                });
            else
                input.series2.push(output.seriesVal2);

            input.LCL2.push(output.lcl2);
            input.UCL2.push(output.ucl2);
            input.LWL2.push(output.lwl2);
            input.UWL2.push(output.uwl2);
            input.xbP2.push(output.xbp2);
        }
    };

    function getCustomxAxisLabel(customInfo) {
        if (customInfo && customInfo.filter) {
            var xAxisObj = customInfo.filter(function (k) { return k.xAxisLabel === true; });
            if (xAxisObj && xAxisObj.length > 0 && xAxisObj[0].value)
                return xAxisObj[0].value;
        }
        return '';
    }

    function getCustomTooltip(customInfo) {
        var customTooltip = [];

        if (customInfo && customInfo.filter) {
            var xAxisObj = customInfo.filter(function (k) { return k.showInTooltip === true; });
            if (xAxisObj) {
                xAxisObj.forEach(function (o) {
                    //customTooltip.push(o.label + ' : ' + o.value);
                    customTooltip.push(tooltipRow.format(o.label, o.value, 'customInfo'));
                });
            }
        }
        // return customTooltip.join('<br>');
        return customTooltip.join('');
    }

    function getBoxplotTooltip() {
        var msg = [],
            inputs = settings.inputs,
            violations = [],
            customInfo = settings.inputs.customInfo || [];

        msg.push(tooltipTableStart);

        if (typeof inputs.series1[this.point.index] === 'number') {
            // msg.push('<b>' + settings.series1Name + ' : ' + inputs.series1[this.point.index] + '</b>');
            msg.push(tooltipRow.format('<b>' + settings.series1Name + '</b>', '<b>' + roundFloat(inputs.series1[this.point.index], settings.decimalPlaces) + '</b>'));
            msg.push(tooltipHr);
        }

        if (inputs.series1[this.point.index] && inputs.series1[this.point.index].violations) {
            // msg.push('<b>' + settings.series1Name + ' : ' + roundFloat(inputs.series1[this.point.index].y, settings.decimalPlaces) + '</b>');
            msg.push(tooltipRow.format('<b>' + settings.series1Name + '</b>', '<b>' + roundFloat(inputs.series1[this.point.index].y, settings.decimalPlaces) + '</b>'));
            msg.push(tooltipHr);
            inputs.series1[this.point.index].violations.forEach(function (v) { violations.push(v); });
        }

        if (typeof inputs.UCL1[this.point.index] === 'number')
            msg.push(tooltipRow.format(getLocalizedText(seriesIds.controlChart.ucl1, settings.locale), roundFloat(inputs.UCL1[this.point.index], settings.decimalPlaces)));
        // msg.push(getLocalizedText(seriesIds.controlChart.ucl1, settings.locale) + ' : ' + inputs.UCL1[this.point.index]);

        if (typeof inputs.UWL1[this.point.index] === 'number')
            msg.push(tooltipRow.format(getLocalizedText(seriesIds.controlChart.uwl1, settings.locale), roundFloat(inputs.UWL1[this.point.index], settings.decimalPlaces)));
        // msg.push(getLocalizedText(seriesIds.controlChart.uwl1, settings.locale) + ' : ' + inputs.UWL1[this.point.index]);

        if (typeof inputs.xbP1[this.point.index] === 'number')
            msg.push(tooltipRow.format(getLocalizedText(seriesIds.controlChart.xbp1, settings.locale), roundFloat(inputs.xbP1[this.point.index], settings.decimalPlaces)));
        // msg.push(getLocalizedText(seriesIds.controlChart.xbp1, settings.locale) + ' : ' + inputs.xbP1[this.point.index]);

        if (typeof inputs.LWL1[this.point.index] === 'number')
            msg.push(tooltipRow.format(getLocalizedText(seriesIds.controlChart.lwl1, settings.locale), roundFloat(inputs.LWL1[this.point.index], settings.decimalPlaces)));
        // msg.push(getLocalizedText(seriesIds.controlChart.lwl1, settings.locale) + ' : ' + inputs.LWL1[this.point.index]);

        if (typeof inputs.LCL1[this.point.index] === 'number')
            msg.push(tooltipRow.format(getLocalizedText(seriesIds.controlChart.lcl1, settings.locale), roundFloat(inputs.LCL1[this.point.index], settings.decimalPlaces)));
        // msg.push(getLocalizedText(seriesIds.controlChart.lcl1, settings.locale) + ' : ' + inputs.LCL1[this.point.index]);

        if (customInfo.length > this.point.index && customInfo[this.point.index].length > 0) {
            // msg.push('<hr style="margin-top: 7px; padding: 0; margin-bottom: -10px; "/>');
            // msg.push(getCustomTooltip(settings.inputs.customInfo[this.point.index]));
            msg.push(tooltipHr);
            msg.push(getCustomTooltip(settings.inputs.customInfo[this.point.index]));
        }

        if (violations.length > 0) {
            // msg.push('<hr style="margin-top: 7px; padding: 0; margin-bottom: -10px; "/>');
            // violations.forEach(function (v) { msg.push(v); });
            msg.push(tooltipHr);
            violations.forEach(function (v) { msg.push(tooltipRow.format(v, "")); });
        }

        // msg.push('<hr style="margin-top: 7px; padding: 0; margin-bottom: -10px; "/>');
        msg.push(tooltipHr);

        if (typeof this.point.high === 'number')
            msg.push(tooltipRow.format(getLocalizedText('bPlot_high', settings.locale), roundFloat(this.point.high, settings.decimalPlaces)));
        //  msg.push(getLocalizedText('bPlot_high', settings.locale) + ' : ' + roundFloat(this.point.high, settings.decimalPlaces));

        if (typeof this.point.q3 === 'number')
            msg.push(tooltipRow.format(getLocalizedText('bPlot_q3', settings.locale), roundFloat(this.point.q3, settings.decimalPlaces)));
        // msg.push(getLocalizedText('bPlot_q3', settings.locale) + ' : ' + roundFloat(this.point.q3, settings.decimalPlaces));

        if (typeof this.point.median === 'number')
            msg.push(tooltipRow.format(getLocalizedText('bPlot_median', settings.locale), roundFloat(this.point.median, settings.decimalPlaces)));
        // msg.push(getLocalizedText('bPlot_median', settings.locale) + ' : ' + roundFloat(this.point.median, settings.decimalPlaces));

        if (typeof this.point.q1 === 'number')
            msg.push(tooltipRow.format(getLocalizedText('bPlot_q1', settings.locale), roundFloat(this.point.q1, settings.decimalPlaces)));
        // msg.push(getLocalizedText('bPlot_q1', settings.locale) + ' : ' + roundFloat(this.point.q1, settings.decimalPlaces));

        if (typeof this.point.low === 'number')
            msg.push(tooltipRow.format(getLocalizedText('bPlot_low', settings.locale), roundFloat(this.point.low, settings.decimalPlaces)));
        // msg.push(getLocalizedText('bPlot_low', settings.locale) + ' : ' + roundFloat(this.point.low, settings.decimalPlaces));

        msg.push(tooltipTableEnd);

        // return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + msg.join('<br>') + "</span></div>"
        return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + msg.join('') + "</span></div>"
    }

    function getCombinedTooltip() {
        var msg = [],
            msgSort = [],
            customInfo = settings.inputs.customInfo || [];

        if (this.points && this.points.length > 0) {
            var violationPoint = null,
                point = this.points[0].point,
                boxplotSeries = null;

            var ucl1 = findSeries(this.points, seriesIds.controlChart.ucl1),
                lcl1 = findSeries(this.points, seriesIds.controlChart.lcl1),
                uwl1 = findSeries(this.points, seriesIds.controlChart.uwl1),
                lwl1 = findSeries(this.points, seriesIds.controlChart.lwl1),
                xbp1 = findSeries(this.points, seriesIds.controlChart.xbp1),
                series1 = findSeries(this.points, seriesIds.controlChart.series1),
                ucl2 = findSeries(this.points, seriesIds.controlChart.ucl2),
                lcl2 = findSeries(this.points, seriesIds.controlChart.lcl2),
                uwl2 = findSeries(this.points, seriesIds.controlChart.uwl2),
                lwl2 = findSeries(this.points, seriesIds.controlChart.lwl2),
                xbp2 = findSeries(this.points, seriesIds.controlChart.xbp2),
                utl1 = findSeries(this.points, seriesIds.controlChart.utl1),
                ltl1 = findSeries(this.points, seriesIds.controlChart.ltl1),

                series2 = findSeries(this.points, seriesIds.controlChart.series2),
                SPCZoneB1 = findSeries(this.points, seriesIds.controlChart.SPCZoneB1),
                SPCZoneB2 = findSeries(this.points, seriesIds.controlChart.SPCZoneB2),
                SPCZoneC1 = findSeries(this.points, seriesIds.controlChart.SPCZoneC1),
                SPCZoneC2 = findSeries(this.points, seriesIds.controlChart.SPCZoneC2),
                SPCZoneA = findSeries(this.points, seriesIds.controlChart.SPCZoneA),

                firstChoiceUpperLimit = findSeries(this.points, seriesIds.controlChart.firstChoiceUpperLimit),
                tt_firstChoiceUpperLimit = getLocalizedText('tt_firstChoiceUpperLimit', settings.locale),
                firstChoiceLowerLimit = findSeries(this.points, seriesIds.controlChart.firstChoiceLowerLimit),
                tt_firstChoiceLowerLimit = getLocalizedText('tt_firstChoiceLowerLimit', settings.locale),
                secondChoiceUpperLimit = findSeries(this.points, seriesIds.controlChart.secondChoiceUpperLimit),
                tt_secondChoiceUpperLimit = getLocalizedText('tt_secondChoiceUpperLimit', settings.locale),
                secondChoiceLowerLimit = findSeries(this.points, seriesIds.controlChart.secondChoiceLowerLimit),
                tt_secondChoiceLowerLimit = getLocalizedText('tt_secondChoiceLowerLimit', settings.locale),
                thirdChoiceUpperLimit = findSeries(this.points, seriesIds.controlChart.thirdChoiceUpperLimit),
                tt_thirdChoiceUpperLimit = getLocalizedText('tt_thirdChoiceUpperLimit', settings.locale),
                thirdChoiceLowerLimit = findSeries(this.points, seriesIds.controlChart.thirdChoiceLowerLimit),
                tt_thirdChoiceLowerLimit = getLocalizedText('tt_thirdChoiceLowerLimit', settings.locale),
                centerOfTolerances = findSeries(this.points, seriesIds.controlChart.centerOfTolerances),
                averageOfAllValues = findSeries(this.points, seriesIds.controlChart.averageOfAllValues),

                oneThirdOfUpperControlLimit = findSeries(this.points, seriesIds.controlChart.oneThirdOfUpperControlLimit),
                tt_oneThirdOfControlLimitsUpper = getLocalizedText('tt_oneThirdOfControlLimitsUpper', settings.locale),

                oneThirdOfLowerControlLimit = findSeries(this.points, seriesIds.controlChart.oneThirdOfLowerControlLimit),
                tt_oneThirdOfControlLimitsLower = getLocalizedText('tt_oneThirdOfControlLimitsLower', settings.locale),

                twoThirdsOfUpperControlLimit = findSeries(this.points, seriesIds.controlChart.twoThirdsOfUpperControlLimit),
                tt_twoThirdsOfControlLimitsUpper = getLocalizedText('tt_twoThirdsOfControlLimitsUpper', settings.locale),

                twoThirdsOfLowerControlLimit = findSeries(this.points, seriesIds.controlChart.twoThirdsOfLowerControlLimit),
                tt_twoThirdsOfControlLimitsLower = getLocalizedText('tt_twoThirdsOfControlLimitsLower', settings.locale),

                calculatedMax = findSeries(this.points, seriesIds.controlChart.calculatedMax),
                tt_calculatedMax = getLocalizedText('bPlot_high', settings.locale),
                calculatedMin = findSeries(this.points, seriesIds.controlChart.calculatedMin),
                tt_calculatedMin = getLocalizedText('bPlot_low', settings.locale),

                nominalValue = findSeries(this.points, seriesIds.controlChart.nominalValue),
                tt_NominalValue = getLocalizedText('tt_NominalValue', settings.locale);

            msg.push(tooltipTableStart);
            msgSort.push(tooltipTableStart);

            if (series1) {
                msg.push(tooltipRow.format('<b>' + series1.series.name + '</b>', '<b>' + roundFloat(series1.y, settings.decimalPlaces) + '</b>', series1.series.userOptions.id));
                msgSort.push(tooltipRow.format('<b>' + series1.series.name + '</b>', '<b>' + roundFloat(series1.y, settings.decimalPlaces) + '</b>', series1.series.userOptions.id));
            }

            if (series2) {
                msg.push(tooltipRow.format('<b>' + series2.series.name + '</b>', '<b>' + roundFloat(series2.y, settings.decimalPlaces) + '</b>', series2.series.userOptions.id));
                msgSort.push(tooltipRow.format('<b>' + series2.series.name + '</b>', '<b>' + roundFloat(series2.y, settings.decimalPlaces) + '</b>', series2.series.userOptions.id));
            }

            if (uwl1 || ucl1 || xbp1 || lcl1 || lwl1 || utl1 || ltl1)
                msg.push(tooltipHr);

            if (uwl1) {
                msg.push(tooltipRow.format(uwl1.series.name, roundFloat(uwl1.y, settings.decimalPlaces), uwl1.series.userOptions.id));
                msgSort.push(tooltipRow.format(uwl1.series.name, roundFloat(uwl1.y, settings.decimalPlaces), uwl1.series.userOptions.id));
            }

            if (ucl1) {
                msg.push(tooltipRow.format(ucl1.series.name, roundFloat(ucl1.y, settings.decimalPlaces, ucl1.series.userOptions.id)));
                msgSort.push(tooltipRow.format(ucl1.series.name, roundFloat(ucl1.y, settings.decimalPlaces, ucl1.series.userOptions.id)));
            }

            if (xbp1) {
                msg.push(tooltipRow.format(xbp1.series.name, roundFloat(xbp1.y, settings.decimalPlaces), xbp1.series.userOptions.id));
                msgSort.push(tooltipRow.format(xbp1.series.name, roundFloat(xbp1.y, settings.decimalPlaces), xbp1.series.userOptions.id));
            }

            if (lcl1) {
                msg.push(tooltipRow.format(lcl1.series.name, roundFloat(lcl1.y, settings.decimalPlaces), lcl1.series.userOptions.id));
                msgSort.push(tooltipRow.format(lcl1.series.name, roundFloat(lcl1.y, settings.decimalPlaces), lcl1.series.userOptions.id));
            }

            if (lwl1) {
                msg.push(tooltipRow.format(lwl1.series.name, roundFloat(lwl1.y, settings.decimalPlaces), lwl1.series.userOptions.id));
                msgSort.push(tooltipRow.format(lwl1.series.name, roundFloat(lwl1.y, settings.decimalPlaces), lwl1.series.userOptions.id));
            }

            if (utl1) {
                msg.push(tooltipRow.format(utl1.series.name, roundFloat(utl1.y, settings.decimalPlaces), utl1.series.userOptions.id));
                msgSort.push(tooltipRow.format(utl1.series.name, roundFloat(utl1.y, settings.decimalPlaces), utl1.series.userOptions.id));
            }

            if (ltl1) {
                msg.push(tooltipRow.format(ltl1.series.name, roundFloat(ltl1.y, settings.decimalPlaces), ltl1.series.userOptions.id));
                msgSort.push(tooltipRow.format(ltl1.series.name, roundFloat(ltl1.y, settings.decimalPlaces), ltl1.series.userOptions.id));
            }

            if (firstChoiceUpperLimit) {
                msg.push(tooltipRow.format(tt_firstChoiceUpperLimit, roundFloat(firstChoiceUpperLimit.y, settings.decimalPlaces, false), firstChoiceUpperLimit.series.userOptions.id));
                msg.push(tooltipRow.format(tt_firstChoiceLowerLimit, roundFloat(firstChoiceLowerLimit.y, settings.decimalPlaces, false), firstChoiceLowerLimit.series.userOptions.id));
                msgSort.push(tooltipRow.format(tt_firstChoiceUpperLimit, roundFloat(firstChoiceUpperLimit.y, settings.decimalPlaces, false), firstChoiceUpperLimit.series.userOptions.id));
                msgSort.push(tooltipRow.format(tt_firstChoiceLowerLimit, roundFloat(firstChoiceLowerLimit.y, settings.decimalPlaces, false), firstChoiceLowerLimit.series.userOptions.id));
            }
            if (secondChoiceUpperLimit) {
                msg.push(tooltipRow.format(tt_secondChoiceUpperLimit, roundFloat(secondChoiceUpperLimit.y, settings.decimalPlaces, false), secondChoiceUpperLimit.series.userOptions.id));
                msg.push(tooltipRow.format(tt_secondChoiceLowerLimit, roundFloat(secondChoiceLowerLimit.y, settings.decimalPlaces, false), secondChoiceLowerLimit.series.userOptions.id));
                msgSort.push(tooltipRow.format(tt_secondChoiceUpperLimit, roundFloat(secondChoiceUpperLimit.y, settings.decimalPlaces, false), secondChoiceUpperLimit.series.userOptions.id));
                msgSort.push(tooltipRow.format(tt_secondChoiceLowerLimit, roundFloat(secondChoiceLowerLimit.y, settings.decimalPlaces, false), secondChoiceLowerLimit.series.userOptions.id));
            }
            if (thirdChoiceUpperLimit) {
                msg.push(tooltipRow.format(tt_thirdChoiceUpperLimit, roundFloat(thirdChoiceUpperLimit.y, settings.decimalPlaces, false), thirdChoiceUpperLimit.series.userOptions.id));
                msg.push(tooltipRow.format(tt_thirdChoiceLowerLimit, roundFloat(thirdChoiceLowerLimit.y, settings.decimalPlaces, false), thirdChoiceLowerLimit.series.userOptions.id));
                msgSort.push(tooltipRow.format(tt_thirdChoiceUpperLimit, roundFloat(thirdChoiceUpperLimit.y, settings.decimalPlaces, false), thirdChoiceUpperLimit.series.userOptions.id));
                msgSort.push(tooltipRow.format(tt_thirdChoiceLowerLimit, roundFloat(thirdChoiceLowerLimit.y, settings.decimalPlaces, false), thirdChoiceLowerLimit.series.userOptions.id));
            }
            if (centerOfTolerances) {
                msg.push(tooltipRow.format(centerOfTolerances.series.name, roundFloat(centerOfTolerances.y, settings.decimalPlaces, false), centerOfTolerances.series.userOptions.id));
                msgSort.push(tooltipRow.format(centerOfTolerances.series.name, roundFloat(centerOfTolerances.y, settings.decimalPlaces, false), centerOfTolerances.series.userOptions.id));
            }

            if (nominalValue) {
                msg.push(tooltipRow.format(tt_NominalValue, roundFloat(nominalValue.y, settings.decimalPlaces, false), nominalValue.series.userOptions.id));
                msgSort.push(tooltipRow.format(tt_NominalValue, roundFloat(nominalValue.y, settings.decimalPlaces, false), nominalValue.series.userOptions.id));
            }
            if (oneThirdOfUpperControlLimit) {
                msg.push(tooltipRow.format(tt_oneThirdOfControlLimitsUpper, roundFloat(oneThirdOfUpperControlLimit.y, settings.decimalPlaces, false), oneThirdOfUpperControlLimit.series.userOptions.id));
                msg.push(tooltipRow.format(tt_oneThirdOfControlLimitsLower, roundFloat(oneThirdOfLowerControlLimit.y, settings.decimalPlaces, false), oneThirdOfLowerControlLimit.series.userOptions.id));
                msgSort.push(tooltipRow.format(tt_oneThirdOfControlLimitsUpper, roundFloat(oneThirdOfUpperControlLimit.y, settings.decimalPlaces, false), oneThirdOfUpperControlLimit.series.userOptions.id));
                msgSort.push(tooltipRow.format(tt_oneThirdOfControlLimitsLower, roundFloat(oneThirdOfLowerControlLimit.y, settings.decimalPlaces, false), oneThirdOfLowerControlLimit.series.userOptions.id));
            }
            if (twoThirdsOfUpperControlLimit) {
                msg.push(tooltipRow.format(tt_twoThirdsOfControlLimitsUpper, roundFloat(twoThirdsOfUpperControlLimit.y, settings.decimalPlaces, false), twoThirdsOfUpperControlLimit.series.userOptions.id));
                msg.push(tooltipRow.format(tt_twoThirdsOfControlLimitsLower, roundFloat(twoThirdsOfLowerControlLimit.y, settings.decimalPlaces, false), twoThirdsOfLowerControlLimit.series.userOptions.id));

                msgSort.push(tooltipRow.format(tt_twoThirdsOfControlLimitsUpper, roundFloat(twoThirdsOfUpperControlLimit.y, settings.decimalPlaces, false), twoThirdsOfUpperControlLimit.series.userOptions.id));
                msgSort.push(tooltipRow.format(tt_twoThirdsOfControlLimitsLower, roundFloat(twoThirdsOfLowerControlLimit.y, settings.decimalPlaces, false), twoThirdsOfLowerControlLimit.series.userOptions.id));
            }

            if (uwl2 || ucl2 || xbp2 || lcl2 || lwl2 || firstChoiceUpperLimit || secondChoiceUpperLimit || thirdChoiceUpperLimit || centerOfTolerances || averageOfAllValues || oneThirdOfUpperControlLimit || twoThirdsOfUpperControlLimit)
                msg.push(tooltipHr);

            if (uwl2) {
                msg.push(tooltipRow.format(uwl2.series.name, roundFloat(uwl2.y, settings.decimalPlaces), uwl2.series.userOptions.id));
                msgSort.push(tooltipRow.format(uwl2.series.name, roundFloat(uwl2.y, settings.decimalPlaces), uwl2.series.userOptions.id));
            }

            if (ucl2) {
                msg.push(tooltipRow.format(ucl2.series.name, roundFloat(ucl2.y, settings.decimalPlaces), ucl2.series.userOptions.id));
                msgSort.push(tooltipRow.format(ucl2.series.name, roundFloat(ucl2.y, settings.decimalPlaces), ucl2.series.userOptions.id));
            }

            if (xbp2) {
                msg.push(tooltipRow.format(xbp2.series.name, roundFloat(xbp2.y, settings.decimalPlaces), xbp2.series.userOptions.id));
                msgSort.push(tooltipRow.format(xbp2.series.name, roundFloat(xbp2.y, settings.decimalPlaces), xbp2.series.userOptions.id));
            }

            if (lcl2) {
                msg.push(tooltipRow.format(lcl2.series.name, roundFloat(lcl2.y, settings.decimalPlaces), lcl2.series.userOptions.id));
                msgSort.push(tooltipRow.format(lcl2.series.name, roundFloat(lcl2.y, settings.decimalPlaces), lcl2.series.userOptions.id));
            }

            if (lwl2) {
                msg.push(tooltipRow.format(lwl2.series.name, roundFloat(lwl2.y, settings.decimalPlaces), lwl2.series.userOptions.id));
                msgSort.push(tooltipRow.format(lwl2.series.name, roundFloat(lwl2.y, settings.decimalPlaces), lwl2.series.userOptions.id));
            }

            if (SPCZoneB2 || SPCZoneB1 || SPCZoneA || SPCZoneC1 || SPCZoneC2)
                msg.push(tooltipHr);

            if (SPCZoneB2) { // +3sigma
                msg.push(tooltipRow.format('+3σ', roundFloat(SPCZoneB2.point.high, settings.decimalPlaces), SPCZoneB2.series.userOptions.id));
                msgSort.push(tooltipRow.format('+3σ', roundFloat(SPCZoneB2.point.high, settings.decimalPlaces), SPCZoneB2.series.userOptions.id));
            }

            if (SPCZoneB1) { // +2Sigma
                msg.push(tooltipRow.format('+2σ', roundFloat(SPCZoneB1.point.high, settings.decimalPlaces), SPCZoneB1.series.userOptions.id));
                msgSort.push(tooltipRow.format('+2σ', roundFloat(SPCZoneB1.point.high, settings.decimalPlaces), SPCZoneB1.series.userOptions.id));
            }

            if (SPCZoneA && SPCZoneA.point) { // Sigma: where is where is -sigma?
                msg.push(tooltipRow.format('+σ', roundFloat(SPCZoneA.point.high, settings.decimalPlaces), SPCZoneA.series.userOptions.id));
                msg.push(tooltipRow.format('-σ', roundFloat(SPCZoneA.point.low, settings.decimalPlaces), SPCZoneA.series.userOptions.id));
                msgSort.push(tooltipRow.format('+σ', roundFloat(SPCZoneA.point.high, settings.decimalPlaces), SPCZoneA.series.userOptions.id));
                msgSort.push(tooltipRow.format('-σ', roundFloat(SPCZoneA.point.low, settings.decimalPlaces), SPCZoneA.series.userOptions.id));
            }

            if (SPCZoneC1) { // -2sigma
                msg.push(tooltipRow.format('-2σ', roundFloat(SPCZoneC1.point.high, settings.decimalPlaces), SPCZoneC1.series.userOptions.id));
                msgSort.push(tooltipRow.format('-2σ', roundFloat(SPCZoneC1.point.high, settings.decimalPlaces), SPCZoneC1.series.userOptions.id));
            }

            if (SPCZoneC2) { // -3sigma
                msg.push(tooltipRow.format('-3σ', roundFloat(SPCZoneC2.point.high, settings.decimalPlaces), SPCZoneC2.series.userOptions.id));
                msgSort.push(tooltipRow.format('-3σ', roundFloat(SPCZoneC2.point.high, settings.decimalPlaces), SPCZoneC2.series.userOptions.id));
            }

            // ==============================
            if (averageOfAllValues) {
                msg.push(tooltipRow.format(averageOfAllValues.series.name, roundFloat(averageOfAllValues.y, settings.decimalPlaces, false), averageOfAllValues.series.userOptions.id));
                msgSort.push(tooltipRow.format(averageOfAllValues.series.name, roundFloat(averageOfAllValues.y, settings.decimalPlaces, false), averageOfAllValues.series.userOptions.id));
            }

            if (calculatedMax) {
                msg.push(tooltipRow.format(tt_calculatedMax, roundFloat(calculatedMax.y, settings.decimalPlaces, false), calculatedMax.series.userOptions.id));
                msgSort.push(tooltipRow.format(tt_calculatedMax, roundFloat(calculatedMax.y, settings.decimalPlaces, false), calculatedMax.series.userOptions.id));
            }
            if (calculatedMin) {
                msg.push(tooltipRow.format(tt_calculatedMin, roundFloat(calculatedMin.y, settings.decimalPlaces, false), calculatedMin.series.userOptions.id));
                msgSort.push(tooltipRow.format(tt_calculatedMin, roundFloat(calculatedMin.y, settings.decimalPlaces, false), calculatedMin.series.userOptions.id));
            }


            $.each(this.points, function (i, ser) {

                if (ser.key.toString().startsWith('violation') || ser.key.toString().startsWith('outlier') || ser.key.toString().startsWith('eliminated')) {
                    violationPoint = ser;
                }

                if (settings.boxplot === true && ser.series.userOptions && ser.series.userOptions.type === 'boxplot') { // it is box plot 
                    boxplotSeries = ser;
                }
            });

            if (customInfo.length > point.index && customInfo[point.index].length > 0) {
                msg.push(tooltipHr);
                msg.push(getCustomTooltip(customInfo[point.index]));
                // msgSort.push(tooltipHr);
                msgSort.push(getCustomTooltip(customInfo[point.index]));
            }

            if (violationPoint && violationPoint.point.violations) {
                msg.push(tooltipHr);
                // msgSort.push(tooltipHr);
                violationPoint.point.violations.forEach(function (v) {
                    msg.push(tooltipViolation.format(v, "", 'violation'));
                    msgSort.push(tooltipViolation.format(v, "", 'violation'));
                });
            }

            if (boxplotSeries) {

                msg.push(tooltipHr);
                msgSort.push(tooltipHr);

                if (typeof boxplotSeries.point.high === 'number') {
                    msg.push(tooltipRow.format(getLocalizedText('bPlot_high', settings.locale), roundFloat(boxplotSeries.point.high, settings.decimalPlaces)));
                    msgSort.push(tooltipRow.format(getLocalizedText('bPlot_high', settings.locale), roundFloat(boxplotSeries.point.high, settings.decimalPlaces)));
                }

                if (typeof boxplotSeries.point.q3 === 'number') {
                    msg.push(tooltipRow.format(getLocalizedText('bPlot_q3', settings.locale), roundFloat(boxplotSeries.point.q3, settings.decimalPlaces)));
                    msgSort.push(tooltipRow.format(getLocalizedText('bPlot_q3', settings.locale), roundFloat(boxplotSeries.point.q3, settings.decimalPlaces)));
                }

                if (typeof boxplotSeries.point.median === 'number') {
                    msg.push(tooltipRow.format(getLocalizedText('bPlot_median', settings.locale), roundFloat(boxplotSeries.point.median, settings.decimalPlaces)));
                    msgSort.push(tooltipRow.format(getLocalizedText('bPlot_median', settings.locale), roundFloat(boxplotSeries.point.median, settings.decimalPlaces)));
                }

                if (typeof boxplotSeries.point.q1 === 'number') {
                    msg.push(tooltipRow.format(getLocalizedText('bPlot_q1', settings.locale), roundFloat(boxplotSeries.point.q1, settings.decimalPlaces)));
                    msgSort.push(tooltipRow.format(getLocalizedText('bPlot_q1', settings.locale), roundFloat(boxplotSeries.point.q1, settings.decimalPlaces)));
                }

                if (typeof boxplotSeries.point.low === 'number') {
                    msg.push(tooltipRow.format(getLocalizedText('bPlot_low', settings.locale), roundFloat(boxplotSeries.point.low, settings.decimalPlaces)));
                    msgSort.push(tooltipRow.format(getLocalizedText('bPlot_low', settings.locale), roundFloat(boxplotSeries.point.low, settings.decimalPlaces)));
                }

            }
            msg.push(tooltipTableEnd);

            // var height = ($this.innerHeight() - 230) + 'px';
            // return "<div style='padding:8px 16px; max-height: {0}; overflow-y: auto; overflow-x: hidden;'><span style='font-size:9pt;font-weight:400'>".format(height) + msg.join('') + "</span></div>";

        } else if (this.series && this.series.name === getLocalizedText('measurements', settings.locale)) {
            msg = [];
            msg.push(this.series.name + ' : ' + this.point.y);
            return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + msg.join('<br>') + "</span></div>";
        }
        else if (this.series && this.series.name === seriesIds.controlChart.flagSeries) {
            msg = [];
            msg.push(getCustomRemarks(settings, this.point.index));
            return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + msg.join('<br>') + "</span></div>";
        }


        if (settings.series && settings.series.tooltip && settings.series.tooltip.length > 0 && this.points && this.points.length > 0) {
            msg = applySeriesOrder(msgSort);
        }

        var height = ($this.innerHeight() - 230) + 'px';
        return "<div style='padding:8px 16px; max-height: {0}; overflow-y: auto; overflow-x: hidden;'><span style='font-size:9pt;font-weight:400'>".format(height) + msg.join('') + "</span></div>";
    }

    function displayTooltip(/*tooltip*/) {
        //ControlChart displayTooltip      
        var msg = [],  // to deal with automatic <br> margin should be adjusted
            customInfo = settings.inputs.customInfo || [];

        msg.push(tooltipTableStart);

        if (settings.splitTooltip === false) {
            return getCombinedTooltip.bind(this)();
        }

        if (settings.boxplot === true && this.point.low) { // it is from boxplot tooltip
            return getBoxplotTooltip.bind(this)();
        }

        // msg.push('<b>' + this.series.name + ' : ' + roundFloat(this.y, settings.decimalPlaces) + '</b></span>');
        msg.push(tooltipRow.format('<b>' + this.series.name + '</b>', '<b>' + roundFloat(this.y, settings.decimalPlaces) + '</b>'));

        if (customInfo.length > this.point.index && customInfo[this.point.index].length > 0) {
            msg.push(tooltipHr);
            msg.push(getCustomTooltip(customInfo[this.point.index]));
        }

        if ((this.key.toString().startsWith('violation') || this.key.toString().startsWith('outlier') || this.key.toString().startsWith('eliminated')) && this.point.violations) {
            // msg.push('<hr style="margin-top: 7px; padding: 0; margin-bottom: -10px; "/>');
            msg.push(tooltipHr);
            this.point.violations.forEach(function (v) { msg.push(tooltipRow.format(v, "")); });
        } else if (this.series.name === seriesIds.controlChart.flagSeries && this.point) {
            // msg = [this.point.text]; // just show annotation
            msg = [];
            msg.push(getCustomRemarks(settings, this.point.x, true));
            // else return tooltip.defaultFormatter.call(this, tooltip);
        }

        if (this.point.text) // for flags type series, only it should be shown
            return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + this.point.text + "</span></div>";

        msg.push(tooltipTableEnd);

        var html = '<div style="padding:8px 16px"><span style="font-size:9pt;font-weight:400">' + msg.join('') + '</span></div>';
        //return tooltip.defaultFormatter.call(this, tooltip);
        return html;
    }

    function prepareSeries(input) {

        var seriesData = [
            {
                id: seriesIds.controlChart.ucl1,
                name: getLocalizedText('UCL_1', settings.locale),
                data: input.UCL1,
                yAxis: 0,
                color: settings.series.color.controlLimit || chartSeriesColors.ControlChart.ControlLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                events: {
                    click: onSeriesClick
                },
                lineWidth: 1.2,
                showInLegend: seriesVisibility(seriesIds.controlChart.ucl1, input.UCL1, settings.hiddenSeries, true).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.ucl1, input.UCL1, settings.hiddenSeries, true).showInChart,
                index: 0
            },
            {
                id: seriesIds.controlChart.uwl1,
                name: getLocalizedText('UWL_1', settings.locale), // Highcharts.uiLocale[settings.locale].UWL_1,
                data: input.UWL1,
                yAxis: 0,
                color: settings.series.color.warningLimit || chartSeriesColors.ControlChart.WarningLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                events: {
                    click: onSeriesClick
                },
                lineWidth: 1.2,
                showInLegend: seriesVisibility(seriesIds.controlChart.uwl1, input.UWL1, settings.hiddenSeries, true).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.uwl1, input.UWL1, settings.hiddenSeries, true).showInChart,
                index: 1
            },
            {
                id: seriesIds.controlChart.series1,
                name: settings.series1Name,
                data: input.series1,
                yAxis: 0,
                color: settings.series.color.series1 || chartSeriesColors.ControlChart.xbarX,
                // showInNavigator: true,  // to display xAxis series in range selector for highstock chart
                showInLegend: false,
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: true,
                    symbol: 'circle',
                    radius: 4
                },
                events: {
                    click: onSeriesClick
                },
                index: 2,
                zoneAxis: 'x',
                zones: prepareColoredZone("involvedbyother_1")
            },
            {
                id: seriesIds.controlChart.xbp1,
                name: settings.meanValueSeries1Name || getLocalizedText('xbp_1', settings.locale), //Highcharts.uiLocale[settings.locale].xbp_1,
                data: input.xbP1,
                yAxis: 0,
                color: settings.series.color.processMeanValue || chartSeriesColors.ControlChart.xbarProcess,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                events: {
                    click: onSeriesClick
                },
                lineWidth: 1.2,
                showInLegend: seriesVisibility(seriesIds.controlChart.xbp1, input.xbP1, settings.hiddenSeries, true).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.xbp1, input.xbP1, settings.hiddenSeries, true).showInChart,
                index: 3
            },
            {
                id: seriesIds.controlChart.lwl1,
                name: getLocalizedText('LWL_1', settings.locale), // Highcharts.uiLocale[settings.locale].LWL_1,
                data: input.LWL1,
                yAxis: 0,
                color: settings.series.color.warningLimit || chartSeriesColors.ControlChart.WarningLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                events: {
                    click: onSeriesClick
                },
                lineWidth: 1.2,
                showInLegend: seriesVisibility(seriesIds.controlChart.lwl1, input.LWL1, settings.hiddenSeries, true).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.lwl1, input.LWL1, settings.hiddenSeries, true).showInChart,
                index: 4
            },
            {
                id: seriesIds.controlChart.lcl1,
                name: getLocalizedText('LCL_1', settings.locale), // Highcharts.uiLocale[settings.locale].LCL_1,
                data: input.LCL1,
                yAxis: 0,
                color: settings.series.color.controlLimit || chartSeriesColors.ControlChart.ControlLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                events: {
                    click: onSeriesClick
                },
                lineWidth: 1.2,
                showInLegend: seriesVisibility(seriesIds.controlChart.lcl1, input.LCL1, settings.hiddenSeries, true).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.lcl1, input.LCL1, settings.hiddenSeries, true).showInChart,
                index: 5
            },
            {
                id: seriesIds.controlChart.ucl2,
                name: getLocalizedText('UCL_2', settings.locale), //  Highcharts.uiLocale[settings.locale].UCL_2,
                data: input.UCL2,
                yAxis: settings.isSeries2Visible ? 1 : 0,
                color: settings.series.color.controlLimit || chartSeriesColors.ControlChart.ControlLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                events: {
                    click: onSeriesClick
                },
                lineWidth: 1.2,

                visible: settings.isSeries2Visible && seriesVisibility(seriesIds.controlChart.ucl2, input.UCL2, settings.hiddenSeries, true).showInChart,
                showInLegend: settings.isSeries2Visible && seriesVisibility(seriesIds.controlChart.ucl2, input.UCL2, settings.hiddenSeries, true).showInLegend,
                index: 6
            },
            {
                id: seriesIds.controlChart.uwl2,
                name: getLocalizedText('UWL_2', settings.locale), // Highcharts.uiLocale[settings.locale].UWL_2,
                data: input.UWL2,
                yAxis: settings.isSeries2Visible ? 1 : 0,
                color: settings.series.color.warningLimit || chartSeriesColors.ControlChart.WarningLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                events: {
                    click: onSeriesClick
                },
                lineWidth: 1.2,
                visible: settings.isSeries2Visible && seriesVisibility(seriesIds.controlChart.uwl2, input.UWL2, settings.hiddenSeries, true).showInChart,
                showInLegend: settings.isSeries2Visible && seriesVisibility(seriesIds.controlChart.uwl2, input.UWL2, settings.hiddenSeries, true).showInLegend,
                index: 7
            },
            {
                id: seriesIds.controlChart.series2,
                name: settings.series2Name,
                data: input.series2,
                yAxis: settings.isSeries2Visible ? 1 : 0,
                visible: settings.isSeries2Visible,
                showInLegend: false,
                color: settings.series.color.series2 || chartSeriesColors.ControlChart.Sr,
                step: false,
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: true,
                    symbol: 'circle',
                    radius: 4
                },
                events: {
                    click: onSeriesClick
                },
                index: 8,
                zoneAxis: 'x',
                zones: prepareColoredZone("involvedbyother_2")
            },
            {
                id: seriesIds.controlChart.xbp2,
                name: settings.meanValueSeries2Name || getLocalizedText('xbp_2', settings.locale), // Highcharts.uiLocale[settings.locale].xbp_2,
                data: input.xbP2,
                yAxis: settings.isSeries2Visible ? 1 : 0,
                color: settings.series.color.processMeanValue || chartSeriesColors.ControlChart.xbarProcess,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                events: {
                    click: onSeriesClick
                },
                lineWidth: 1.2,
                visible: seriesVisibility(seriesIds.controlChart.xbp2, input.xbP2, settings.hiddenSeries, true).showInChart,
                showInLegend: seriesVisibility(seriesIds.controlChart.xbp2, input.xbP2, settings.hiddenSeries, true).showInLegend,
                index: 9
            },
            {
                id: seriesIds.controlChart.lwl2,
                name: getLocalizedText('LWL_2', settings.locale), // Highcharts.uiLocale[settings.locale].LWL_2,
                data: input.LWL2,
                yAxis: settings.isSeries2Visible ? 1 : 0,
                color: settings.series.color.warningLimit || chartSeriesColors.ControlChart.WarningLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                events: {
                    click: onSeriesClick
                },
                lineWidth: 1.2,
                visible: seriesVisibility(seriesIds.controlChart.lwl2, input.LWL2, settings.hiddenSeries, true).showInChart,
                showInLegend: seriesVisibility(seriesIds.controlChart.lwl2, input.LWL2, settings.hiddenSeries, true).showInLegend,
                index: 10
            },
            {
                id: seriesIds.controlChart.lcl2,
                name: getLocalizedText('LCL_2', settings.locale),
                data: input.LCL2,
                yAxis: settings.isSeries2Visible ? 1 : 0,
                color: settings.series.color.controlLimit || chartSeriesColors.ControlChart.ControlLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                events: {
                    click: onSeriesClick
                },
                lineWidth: 1.2,
                visible: seriesVisibility(seriesIds.controlChart.lcl2, input.LCL2, settings.hiddenSeries, true).showInChart,
                showInLegend: seriesVisibility(seriesIds.controlChart.lcl2, input.LCL2, settings.hiddenSeries, true).showInLegend,
                index: 11
            },
            {
                id: seriesIds.controlChart.flagSeries,
                name: seriesIds.controlChart.flagSeries,
                type: 'flags',
                data: input.flagSeries,
                onSeries: settings.series1Name,
                showInLegend: false,
                useHTML: true,
                dataLabels: {
                    useHTML: true,
                },
                lineWidth: 1,
                lineColor: '#005F87',
                stackDistance: 10

            },
            {
                id: seriesIds.controlChart.firstChoiceUpperLimit,
                name: getLocalizedText('firstChoiceUpperLimit', settings.locale), //  Highcharts.uiLocale[settings.locale].firstChoiceUpperLimit,
                data: input.firstChoiceUpperLimit,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.FirstChoice,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 9,
                showInLegend: seriesVisibility(seriesIds.controlChart.firstChoiceUpperLimit, input.firstChoiceUpperLimit, settings.hiddenSeries, false).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.firstChoiceUpperLimit, input.firstChoiceUpperLimit, settings.hiddenSeries, false).showInChart,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.firstChoiceLowerLimit,
                linkedTo: seriesIds.controlChart.firstChoiceUpperLimit,
                //name: getLocalizedText('firstChoiceLowerLimit', settings.locale), //  Highcharts.uiLocale[settings.locale].firstChoiceLowerLimit,
                data: input.firstChoiceLowerLimit,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.FirstChoice,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 10,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.secondChoiceUpperLimit,
                name: getLocalizedText('secondChoiceUpperLimit', settings.locale), //  Highcharts.uiLocale[settings.locale].secondChoiceUpperLimit,
                data: input.secondChoiceUpperLimit,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.SecondChoice,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 11,
                showInLegend: seriesVisibility(seriesIds.controlChart.secondChoiceUpperLimit, input.secondChoiceUpperLimit, settings.hiddenSeries, false).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.secondChoiceUpperLimit, input.secondChoiceUpperLimit, settings.hiddenSeries, false).showInChart,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.secondChoiceLowerLimit,
                linkedTo: seriesIds.controlChart.secondChoiceUpperLimit,
                //name: getLocalizedText('secondChoiceLowerLimit', settings.locale), //  Highcharts.uiLocale[settings.locale].secondChoiceLowerLimit,
                data: input.secondChoiceLowerLimit,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.SecondChoice,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 12,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.thirdChoiceUpperLimit,
                name: getLocalizedText('thirdChoiceUpperLimit', settings.locale), //  Highcharts.uiLocale[settings.locale].firstChoiceLowerLimit,
                data: input.thirdChoiceUpperLimit,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.ThirdChoice,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 13,
                showInLegend: seriesVisibility(seriesIds.controlChart.thirdChoiceUpperLimit, input.thirdChoiceUpperLimit, settings.hiddenSeries, false).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.thirdChoiceUpperLimit, input.thirdChoiceUpperLimit, settings.hiddenSeries, false).showInChart,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.thirdChoiceLowerLimit,
                linkedTo: seriesIds.controlChart.thirdChoiceUpperLimit,
                //name: getLocalizedText('thirdChoiceLowerLimit', settings.locale), //  Highcharts.uiLocale[settings.locale].thirdChoiceLowerLimit,
                data: input.thirdChoiceLowerLimit,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.ThirdChoice,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 14,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.centerOfTolerances,
                name: getLocalizedText('centerOfTolerances', settings.locale), //  Highcharts.uiLocale[settings.locale].centerOfTolerances,
                data: input.centerOfTolerances,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.CenterOfTolerances,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 15,
                showInLegend: seriesVisibility(seriesIds.controlChart.centerOfTolerances, input.centerOfTolerances, settings.hiddenSeries, false).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.centerOfTolerances, input.centerOfTolerances, settings.hiddenSeries, false).showInChart,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.averageOfAllValues,
                name: getLocalizedText('averageOfAllValues', settings.locale), //  Highcharts.uiLocale[settings.locale].averageOfAllValues,
                data: input.averageOfAllValues,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.averageOfAllValues,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 16,
                showInLegend: seriesVisibility(seriesIds.controlChart.averageOfAllValues, input.oneThirdOfUpperControlLimit, settings.hiddenSeries, false).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.averageOfAllValues, input.oneThirdOfUpperControlLimit, settings.hiddenSeries, false).showInChart,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.oneThirdOfUpperControlLimit,
                name: getLocalizedText('oneThirdOfControlLimits', settings.locale), //  Highcharts.uiLocale[settings.locale].oneThirdOfUpperControlLimit,
                data: input.oneThirdOfUpperControlLimit,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.oneThirdOfControlLimits,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 17,
                showInLegend: seriesVisibility(seriesIds.controlChart.oneThirdOfUpperControlLimit, input.oneThirdOfUpperControlLimit, settings.hiddenSeries, false).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.oneThirdOfUpperControlLimit, input.oneThirdOfUpperControlLimit, settings.hiddenSeries, false).showInChart,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.twoThirdsOfUpperControlLimit,
                name: getLocalizedText('twoThirdsOfControlLimits', settings.locale), //  Highcharts.uiLocale[settings.locale].twoThirdsOfUpperControlLimit,
                data: input.twoThirdsOfUpperControlLimit,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.twoThirdsOfControlLimits,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 17,
                showInLegend: seriesVisibility(seriesIds.controlChart.twoThirdsOfUpperControlLimit, input.twoThirdsOfUpperControlLimit, settings.hiddenSeries, false).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.twoThirdsOfUpperControlLimit, input.twoThirdsOfUpperControlLimit, settings.hiddenSeries, false).showInChart,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.oneThirdOfLowerControlLimit,
                linkedTo: seriesIds.controlChart.oneThirdOfUpperControlLimit,
                data: input.oneThirdOfLowerControlLimit,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.oneThirdOfControlLimits,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 18,
                showInLegend: false,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.twoThirdsOfLowerControlLimit,
                linkedTo: seriesIds.controlChart.twoThirdsOfUpperControlLimit,
                data: input.twoThirdsOfLowerControlLimit,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.twoThirdsOfControlLimits,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 18,
                showInLegend: false,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.calculatedMax,
                name: getLocalizedText('bPlot_high', settings.locale), //  Highcharts.uiLocale[settings.locale].secondChoiceUpperLimit,
                data: input.calculatedMax,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.calculatedMax,
                // step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 11,
                showInLegend: seriesVisibility(seriesIds.controlChart.calculatedMax, input.calculatedMax, settings.hiddenSeries, false).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.calculatedMax, input.calculatedMax, settings.hiddenSeries, false).showInChart,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.calculatedMin,
                name: getLocalizedText('bPlot_low', settings.locale), //  Highcharts.uiLocale[settings.locale].secondChoiceUpperLimit,
                data: input.calculatedMin,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.calculatedMin,
                // step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 11,
                showInLegend: seriesVisibility(seriesIds.controlChart.calculatedMin, input.calculatedMin, settings.hiddenSeries, false).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.calculatedMin, input.calculatedMin, settings.hiddenSeries, false).showInChart,
                events: {
                    click: onSeriesClick
                }
            },
            {
                id: seriesIds.controlChart.nominalValue,
                name: getLocalizedText('nominalValue', settings.locale), //  Highcharts.uiLocale[settings.locale].firstChoiceLowerLimit,
                data: input.nominalValue,
                yAxis: 0,
                color: chartSeriesColors.ControlChart.FirstChoice,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                showInLegend: seriesVisibility(seriesIds.controlChart.nominalValue, input.nominalValue, settings.hiddenSeries, false).showInLegend,
                visible: seriesVisibility(seriesIds.controlChart.nominalValue, input.nominalValue, settings.hiddenSeries, false).showInChart,
                marker: {
                    enabled: false
                },
                lineWidth: 1.2,
                legendIndex: 10,
                events: {
                    click: onSeriesClick
                }
            },
        ];

        if (settings.enableTolerenceLimits === true) {
            seriesData.push(
                {
                    id: seriesIds.controlChart.utl1,
                    name: getLocalizedText('UTL_1', settings.locale),
                    data: input.UTL1,
                    yAxis: 0,
                    color: settings.series.color.toleranceLimit || chartSeriesColors.ControlChart.ToleranceLimit,
                    step: 'left',
                    tooltip: {
                        valueDecimals: settings.decimalPlaces
                    },
                    marker: {
                        enabled: false
                    },
                    events: {
                        click: onSeriesClick
                    },
                    lineWidth: 1.2,
                    showInLegend: seriesVisibility(seriesIds.controlChart.utl1, input.UTL1, settings.hiddenSeries, true).showInLegend,
                    visible: seriesVisibility(seriesIds.controlChart.utl1, input.UTL1, settings.hiddenSeries, true).showInChart
                });

            seriesData.push(
                {
                    id: seriesIds.controlChart.ltl1,
                    name: getLocalizedText('LTL_1', settings.locale),
                    data: input.LTL1,
                    yAxis: 0,
                    color: settings.series.color.toleranceLimit || chartSeriesColors.ControlChart.ToleranceLimit,
                    step: 'left',
                    tooltip: {
                        valueDecimals: settings.decimalPlaces
                    },
                    marker: {
                        enabled: false
                    },
                    events: {
                        click: onSeriesClick
                    },
                    lineWidth: 1.2,
                    showInLegend: seriesVisibility(seriesIds.controlChart.ltl1, input.LTL1, settings.hiddenSeries, true).showInLegend,
                    visible: seriesVisibility(seriesIds.controlChart.ltl1, input.LTL1, settings.hiddenSeries, true).showInChart
                });
        }

        if (settings.boxplot === true) {
            seriesData.push({
                linkedTo: seriesIds.controlChart.boxplot, // 'boxplot',
                name: getLocalizedText('bPlot_series', settings.locale) + '_orig',
                data: input.boxplot,
                type: 'boxplot',
                id: seriesIds.controlChart.boxplot_main,
                showInLegend: false
            }, { // dummy series to show box icon and linked to boxplot series; as boxplot legend icon is not customizeable
                name: getLocalizedText('bPlot_series', settings.locale),
                id: seriesIds.controlChart.boxplot, // for custom icon in legend
                data: null,
                type: 'scatter',
                marker: {
                    enabled: true,
                    lineColor: 'grey',
                    symbol: 'box',
                    lineWidth: 2,
                    radius: 5
                },
                visible: seriesVisibility(seriesIds.controlChart.boxplot, input.boxplot, settings.hiddenSeries, true).showInChart,
                showInLegend: seriesVisibility(seriesIds.controlChart.boxplot, input.boxplot, settings.hiddenSeries, true).showInLegend
            });
        }

        if (settings.drawSPCZones === true && isRelevantSpcZoneChartType()) {
            seriesData.push(
                {
                    id: seriesIds.controlChart.SPCZoneA,
                    name: '± Sigma',
                    type: 'arearange',
                    data: input.zoneA,
                    zIndex: -1,
                    fillOpacity: 0.4,
                    showInLegend: seriesVisibility(seriesIds.controlChart.SPCZoneA, input.zoneA, settings.hiddenSeries, true).showInLegend,
                    visible: seriesVisibility(seriesIds.controlChart.SPCZoneA, input.zoneA, settings.hiddenSeries, true).showInChart
                },
                {
                    id: seriesIds.controlChart.SPCZoneB1,
                    name: '± 2Sigma',
                    type: 'arearange',
                    data: input.zoneB1,
                    zIndex: -1,
                    fillOpacity: 0.4,
                    color: '#BDF5B1',
                    showInLegend: seriesVisibility(seriesIds.controlChart.SPCZoneB1, input.zoneB1, settings.hiddenSeries, true).showInLegend,
                    visible: seriesVisibility(seriesIds.controlChart.SPCZoneB1, input.zoneB1, settings.hiddenSeries, true).showInChart
                },
                {
                    id: seriesIds.controlChart.SPCZoneC1,
                    name: '± 2Sigma',
                    type: 'arearange',
                    data: input.zoneC1,
                    zIndex: -1,
                    fillOpacity: 0.4,
                    color: '#BDF5B1',
                    showInLegend: false,
                    linkedTo: seriesIds.controlChart.SPCZoneB1
                },
                {
                    id: seriesIds.controlChart.SPCZoneB2,
                    name: '± 3Sigma',
                    type: 'arearange',
                    data: input.zoneB2,
                    zIndex: -1,
                    fillOpacity: 0.4,
                    color: '#B3B6F2',
                    showInLegend: seriesVisibility(seriesIds.controlChart.SPCZoneB2, input.zoneB2, settings.hiddenSeries, true).showInLegend,
                    visible: seriesVisibility(seriesIds.controlChart.SPCZoneB2, input.zoneB2, settings.hiddenSeries, true).showInChart
                },
                {
                    id: seriesIds.controlChart.SPCZoneC2,
                    name: '± 3Sigma',
                    type: 'arearange',
                    data: input.zoneC2,
                    zIndex: -1,
                    fillOpacity: 0.4,
                    color: '#B3B6F2',
                    showInLegend: false,
                    linkedTo: seriesIds.controlChart.SPCZoneB2
                }
            )

        }

        if (displayMeasurements()) {
            seriesData.push({
                name: getLocalizedText('measurements', settings.locale),
                data: input.measurements,
                type: 'scatter',
                id: seriesIds.controlChart.measurement,
                // showInLegend: true,
                visible: seriesVisibility(seriesIds.controlChart.measurement, input.measurements, settings.hiddenSeries, true).showInChart,
                showInLegend: seriesVisibility(seriesIds.controlChart.measurement, input.measurements, settings.hiddenSeries, true).showInLegend,
                marker: {
                    enabled: true,
                    symbol: 'cross',
                    // fillColor: 'grey',
                    lineWidth: 2,
                    radius: 3,
                    lineColor: 'grey'
                }
            });
        }
        return seriesData;
    }

    function setDefault(_options) {
        var opt = undefined;
        if (_options && typeof _options === 'object') {


            opt = $.extend({}, _options);
            opt.seriesNames = opt.chartType ? opt.chartType.split('_') : ['', '']; // to display the names in yAxis
            opt.siemensTooltip = $.extend({}, siemensTooltip);
            opt.siemensTooltip.formatter = displayTooltip;
            if (parseMovingMeanWeighting(opt.data.specifications.movingMeanWeighting) === movingMeanWeightings.Exponentially) {
                opt.seriesNames[0] = 'ewma';
                opt.boxplot = false;
            }
            opt.chartType = parseChartType(opt.chartType);
            if (!opt.locale || !Highcharts.uiLocale[opt.locale]) {
                console.error('Warning: Locale "' + opt.locale + '" not found, default English locales will be used');
                opt.locale = 'en';
            }

            if (!opt.series && typeof opt.series !== 'object') opt.series = getDefaultSeriesSettings();
            else opt.series = $.extend(getDefaultSeriesSettings(), opt.series);

            if (typeof opt.yAxisUnit !== 'object') opt.yAxisUnit = getDefaultyAxisUnitObj();
            else opt.yAxisUnit = $.extend(getDefaultyAxisUnitObj(), opt.yAxisUnit);

            if (_options.splitTooltip === false/* && _options.siemensTooltip*/) {
                opt.siemensTooltip.split = true; // will combined the tooltip in highstock                
            }

            if (_options.fontSize && _options.fontSize.labels) {
                axiesTitleStyle.fontSize = _options.fontSize.labels;
            }
            if (_options.fontSize && _options.fontSize.title) {
                titleStyle.fontSize = _options.fontSize.title;
            }

            if (_options.titleSettings) {
                _options.titleSettings.titleStyle = $.extend({}, titleStyle);
                _options.titleSettings.titleStyle.transform = 'translate(' + _options.titleSettings.x + 'px, ' + _options.titleSettings.y + 'px) rotate(' + _options.titleSettings.rotate + 'deg)'
            }

        }

        return opt;
    }
    

    function calculateLimits(input, movingMeanWeightingType) {
        var output = {};

        if (input) {
            switch (settings.chartType) {
                case chartTypes.s:
                case chartTypes.R:
                case chartTypes.ms:
                case chartTypes.mR:
                    output.ucl1 = roundFloat(input.upperControlLimit2Abs, true);
                    output.lcl1 = roundFloat(input.lowerControlLimit2Abs, true);
                    output.uwl1 = roundFloat(input.upperWarnLimit2Abs, true);
                    output.lwl1 = roundFloat(input.lowerWarnLimit2Abs, true);
                    output.xbp1 = roundFloat(input.calculatedProcessMeanValue2, true);
                    output.seriesVal1 = settings.chartType === chartTypes.s || settings.chartType === chartTypes.ms ? roundFloat(input.calculatedS, true) : roundFloat(input.calculatedR, true);
                    break;
                case chartTypes.xb_s:
                case chartTypes.mxb_ms:
                case chartTypes.x_ms:
                case chartTypes.xb_R:
                case chartTypes.mxb_mR:
                case chartTypes.x_mR:
                case chartTypes.cu_Sum:
                case chartTypes.med_R:
                case chartTypes.mxb:
                    output.ucl1 = roundFloat(input.upperControlLimit1Abs, true);
                    output.lcl1 = roundFloat(input.lowerControlLimit1Abs, true);
                    output.uwl1 = roundFloat(input.upperWarnLimit1Abs, true);
                    output.lwl1 = roundFloat(input.lowerWarnLimit1Abs, true);
                    output.xbp1 = roundFloat(input.calculatedProcessMeanValue1, true);
                    output.utl1 = roundFloat(input.upperToleranceLimitAbs, null, true); // only for upper chart
                    output.ltl1 = roundFloat(input.lowerToleranceLimitAbs, null, true); // only for upper chart
                    output.seriesVal1 = movingMeanWeightingType === movingMeanWeightings.Exponentially ? roundFloat(input.calculatedEWMA, true) : roundFloat(input.calculatedXb, true);
                    output.ucl2 = roundFloat(input.upperControlLimit2Abs, true);
                    output.lcl2 = roundFloat(input.lowerControlLimit2Abs, true);
                    output.uwl2 = roundFloat(input.upperWarnLimit2Abs, true);
                    output.lwl2 = roundFloat(input.lowerWarnLimit2Abs, true);
                    output.xbp2 = roundFloat(input.calculatedProcessMeanValue2, true);
                    output.seriesVal2 = settings.chartType === chartTypes.xb_s || settings.chartType === chartTypes.mxb_ms || settings.chartType === chartTypes.x_ms ? roundFloat(input.calculatedS, true) : roundFloat(input.calculatedR, true);
                    break;
                case chartTypes.med:
                case chartTypes.xb:
                case chartTypes.x:
                    output.ucl1 = roundFloat(input.upperControlLimit1Abs, true);
                    output.uwl1 = roundFloat(input.upperWarnLimit1Abs, true);
                    output.seriesVal1 = roundFloat(input.calculatedXb, true);
                    output.xbp1 = roundFloat(input.calculatedProcessMeanValue1, true);
                    output.lwl1 = roundFloat(input.lowerWarnLimit1Abs, true);
                    output.lcl1 = roundFloat(input.lowerControlLimit1Abs, true);
                    output.utl1 = roundFloat(input.upperToleranceLimitAbs, null, true); // only for upper chart
                    output.ltl1 = roundFloat(input.lowerToleranceLimitAbs, null, true); // only for upper chart
                    break;
                default: // XB chart type
                    output.ucl1 = roundFloat(input.upperControlLimit1Abs, true);
                    output.lcl1 = roundFloat(input.lowerControlLimit1Abs, true);
                    output.uwl1 = roundFloat(input.upperWarnLimit1Abs, true);
                    output.lwl1 = roundFloat(input.lowerWarnLimit1Abs, true);
                    output.xbp1 = roundFloat(input.calculatedProcessMeanValue1, true);
                    output.seriesVal1 = roundFloat(input.calculatedXb, true);
            }

            if (settings.boxplot === true) {
                output.boxplot = [];
                $.each(input.boxPlot, function (i, d) {
                    output.boxplot.push(roundFloat(d));
                });
            }
        }

        return output;
    }

    function prepareChartOptions(seriesData, input) {
        var options =
        {
            chart: {
                backgroundColor: settings.backgroundColor,
                type: 'line',
                height: settings.height,
                width: settings.width,
                animation: settings.animation,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                },
                events: {
                    load: onChartLoaded
                }
            },
            exporting: {
                enabled: false
            },
            credits: {
                enabled: false
            },
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            plotOptions: {
                series: {
                    animation: settings.animation,
                    states: {
                        inactive: {
                            opacity: 0.8

                        },

                    },
                    marker: {
                        states: {
                            hover: {
                                enabled: false
                            }
                        }
                    },
                    events: {
                        legendItemClick: onLegendClick
                    }
                },
                boxplot: {
                    color: 'grey',
                    fillColor: '#E1DCDC12',
                    lineWidth: 1,
                    medianColor: '#0C5DA5',
                    medianWidth: 2
                },
                area: {
                    stacking: 'normal',
                    lineColor: '#666666',
                    lineWidth: 1,
                    marker: {
                        lineWidth: 1,
                        lineColor: '#666666'
                    }
                }
            },
            navigator: {
                baseSeries: 2,
                enabled: true, // to show or hide zoom level on x-axis
                series: {
                    lineColor: chartSeriesColors.ControlChart.Measurement
                },
                xAxis: {
                    labels: {
                        formatter: function () {
                            return this.value;
                        }
                    }
                }
            },
            rangeSelector: {
                enabled: false
            },
            title: {
                text: settings.chartTitle,
                align: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.align)) ? 'center' : settings.titleSettings.align,
                style: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.titleStyle)) ? titleStyle : settings.titleSettings.titleStyle,
            },
            tooltip: settings.siemensTooltip,
            xAxis: prepareXAxis(input),
            yAxis: yAxisLabels(),
            series: seriesData
        };
        validateLegendPositioning(options.legend);
        return options;
    }


    function prepareXAxis(input) {
        var end = input.subgroups.length - 1;
        var start = end - 25;
        if (start < 0) {
            start = 0;
        }
        var xAxis = {
            lineColor: siemensColors.PL_BLACK_22,
            lineWidth: 1,
            gridLineColor: siemensColors.PL_BLACK_22,
            title: {
                text: settings.xAxisTitle,
                style: axiesTitleStyle
            },

            labels: {
                rotation: settings.xAxisLabelsRotation,
                style: {
                    color: settings.xAxis.labels.color,
                    fontSize: settings.xAxis.labels.fontSize
                },
                formatter: function () {
                    return input.subgroups[this.value];
                }
            },

            min: start,
            max: end,
            range: 25, // select last 25 samples
            events: {
                afterSetExtremes: onNavigationChanged
            }
        }
        return xAxis;
    }

    function addNewSubgroup(newSubgroup, redraw) {

        var chartApi = $this.data('chartApi');

        if (newSubgroup && chartApi) {
            extractSubgroupData(newSubgroup, settings.inputs.series1.length, settings.inputs);

            if (chartApi.get(seriesIds.controlChart.series1))
                chartApi.get(seriesIds.controlChart.series1).setData(settings.inputs.series1, false);

            if (chartApi.get(seriesIds.controlChart.ucl1))
                chartApi.get(seriesIds.controlChart.ucl1).setData(settings.inputs.UCL1, false);

            if (chartApi.get(seriesIds.controlChart.lcl1))
                chartApi.get(seriesIds.controlChart.lcl1).setData(settings.inputs.LCL1, false);

            if (chartApi.get(seriesIds.controlChart.uwl1))
                chartApi.get(seriesIds.controlChart.uwl1).setData(settings.inputs.UWL1, false);

            if (chartApi.get(seriesIds.controlChart.lwl1))
                chartApi.get(seriesIds.controlChart.lwl1).setData(settings.inputs.LWL1, false);

            if (chartApi.get(seriesIds.controlChart.xbp1))
                chartApi.get(seriesIds.controlChart.xbp1).setData(settings.inputs.xbP1, false);

            if (chartApi.get(seriesIds.controlChart.toolChanged))
                chartApi.get(seriesIds.controlChart.toolChanged).setData(settings.inputs.flagSeries, false);

            if (settings.boxplot === true) {
                chartApi.get(seriesIds.controlChart.boxplot_main).setData(settings.inputs.boxplot, false);
            }

            if (settings.isSeries2Visible) {
                if (chartApi.get(seriesIds.controlChart.series2))
                    chartApi.get(seriesIds.controlChart.series2).setData(settings.inputs.series2, false);

                if (chartApi.get(seriesIds.controlChart.ucl2))
                    chartApi.get(seriesIds.controlChart.ucl2).setData(settings.inputs.UCL2, false);

                if (chartApi.get(seriesIds.controlChart.lcl2))
                    chartApi.get(seriesIds.controlChart.lcl2).setData(settings.inputs.LCL2, false);

                if (chartApi.get(seriesIds.controlChart.uwl2))
                    chartApi.get(seriesIds.controlChart.uwl2).setData(settings.inputs.UWL2, false);

                if (chartApi.get(seriesIds.controlChart.lwl2))
                    chartApi.get(seriesIds.controlChart.lwl2).setData(settings.inputs.LWL2, false);

                if (chartApi.get(seriesIds.controlChart.xbp2))
                    chartApi.get(seriesIds.controlChart.xbp2).setData(settings.inputs.xbP2, false);
            }
        }
        if (redraw)
            chartApi.redraw();
    }

    function updateChartColor(colorScheme) {
        var chartApi = $this.data('chartApi');

        if (typeof colorScheme === 'object' && chartApi) {
            if (colorScheme.backgroundColor) {
                chartApi.update({ chart: { backgroundColor: colorScheme.backgroundColor } }, false);
            }

            if (colorScheme.series1) {
                updateSeriesColor(seriesIds.controlChart.series1, colorScheme.series1);
            }

            if (colorScheme.series2) {
                updateSeriesColor(seriesIds.controlChart.series2, colorScheme.series2);
            }

            if (colorScheme.controlLimit) {
                updateSeriesColor(seriesIds.controlChart.ucl1, colorScheme.controlLimit);
                updateSeriesColor(seriesIds.controlChart.lcl1, colorScheme.controlLimit);
                updateSeriesColor(seriesIds.controlChart.ucl2, colorScheme.controlLimit);
                updateSeriesColor(seriesIds.controlChart.lcl2, colorScheme.controlLimit);
            }

            if (colorScheme.warningLimit) {
                updateSeriesColor(seriesIds.controlChart.uwl1, colorScheme.warningLimit);
                updateSeriesColor(seriesIds.controlChart.lwl1, colorScheme.warningLimit);
                updateSeriesColor(seriesIds.controlChart.uwl2, colorScheme.warningLimit);
                updateSeriesColor(seriesIds.controlChart.lwl2, colorScheme.warningLimit);
            }

            if (colorScheme.processMeanValue) {
                updateSeriesColor(seriesIds.controlChart.xbp1, colorScheme.processMeanValue);
                updateSeriesColor(seriesIds.controlChart.xbp2, colorScheme.processMeanValue);
            }

            chartApi.redraw();
        }

        function updateSeriesColor(seriesId, newColor) {
            var chartApi = $this.data('chartApi');

            if (chartApi && chartApi.get(seriesId) && newColor) {
                chartApi.get(seriesId).update({ color: newColor }, false);
            }
        }
    }

    function updateSubgroup(sgNum, subGroupObj) {
        var chartApi = $this.data('chartApi');
        //GetSubGroup obj
        if (subGroupObj.customInfo) {
            settings.inputs.customInfo[sgNum] = subGroupObj.customInfo;
            var flagSeriesData = chartApi.get(seriesIds.controlChart.flagremarks).data;
            if (flagSeriesData) {
                flagSeriesData.filter(function (obj) {
                    return obj.x === sgNum
                }).map(function (obj) {
                    obj.text = getCustomRemarks(settings, sgNum);
                });
            }

        }
        if (isNullOrUndefined(subGroupObj.lcl1) && isNullOrUndefined(chartApi.get(seriesIds.controlChart.lcl1).data[sgNum])) {
            var lcl1 = roundFloat(subGroupObj.lcl1, settings.decimalPlaces);
            settings.inputs.LCL1[sgNum] = lcl1;
            chartApi.get(seriesIds.controlChart.lcl1).data[sgNum].update(lcl1);

        }
        if (isNullOrUndefined(subGroupObj.ucl1) && isNullOrUndefined(chartApi.get(seriesIds.controlChart.ucl1).data[sgNum])) {
            var ucl1 = roundFloat(subGroupObj.ucl1, settings.decimalPlaces);
            settings.inputs.UCL1[sgNum] = ucl1;
            chartApi.get(seriesIds.controlChart.ucl1).data[sgNum].update(ucl1);

        }
        if (isNullOrUndefined(subGroupObj.lwl1) && isNullOrUndefined(chartApi.get(seriesIds.controlChart.lwl1).data[sgNum])) {
            var lwl1 = roundFloat(subGroupObj.lwl1, settings.decimalPlaces);
            settings.inputs.LWL1[sgNum] = lwl1;
            chartApi.get(seriesIds.controlChart.lwl1).data[sgNum].update(lwl1);

        }
        if (isNullOrUndefined(subGroupObj.uwl1) && isNullOrUndefined(chartApi.get(seriesIds.controlChart.uwl1).data[sgNum])) {
            var uwl1 = roundFloat(subGroupObj.uwl1, settings.decimalPlaces);
            settings.inputs.UWL1[sgNum] = uwl1;
            chartApi.get(seriesIds.controlChart.uwl1).data[sgNum].update(uwl1);

        }
        if (isNullOrUndefined(subGroupObj.xbP1) && isNullOrUndefined(chartApi.get(seriesIds.controlChart.xbP1).data[sgNum])) {
            var xbP1 = roundFloat(subGroupObj.xbP1, settings.decimalPlaces);
            settings.inputs.xbP1[sgNum] = xbP1;
            chartApi.get(seriesIds.controlChart.xbp1).data[sgNum].update(xbP1);

        }
        if (isNullOrUndefined(subGroupObj.series1) && isNullOrUndefined(chartApi.get(seriesIds.controlChart.series1).data[sgNum])) {
            var series1 = roundFloat(subGroupObj.series1, settings.decimalPlaces);
            settings.inputs.series1[sgNum] = series1;
            chartApi.get(seriesIds.controlChart.series1).data[sgNum].update(series1);

        }

        if (isNullOrUndefined(subGroupObj.lcl2) && isNullOrUndefined(chartApi.get(seriesIds.controlChart.lcl2).data[sgNum])) {
            var lcl2 = roundFloat(subGroupObj.lcl2, settings.decimalPlaces);
            settings.inputs.LCL2[sgNum] = lcl2;
            chartApi.get(seriesIds.controlChart.lcl2).data[sgNum].update(lcl2);

        }
        if (isNullOrUndefined(subGroupObj.ucl2) && isNullOrUndefined(chartApi.get(seriesIds.controlChart.ucl2).data[sgNum])) {
            var ucl2 = roundFloat(subGroupObj.ucl2, settings.decimalPlaces);
            settings.inputs.UCL2[sgNum] = ucl2;
            chartApi.get(seriesIds.controlChart.ucl2).data[sgNum].update(ucl2);

        }
        if (isNullOrUndefined(subGroupObj.lwl2) && isNullOrUndefined(chartApi.get(seriesIds.controlChart.lwl2).data[sgNum])) {
            var lwl2 = roundFloat(subGroupObj.lwl2, settings.decimalPlaces);
            settings.inputs.LWL2[sgNum] = lwl2;
            chartApi.get(seriesIds.controlChart.lwl2).data[sgNum].update(lwl2);

        }
        if (isNullOrUndefined(subGroupObj.uwl2) && isNullOrUndefined(chartApi.get(seriesIds.controlChart.uwl2).data[sgNum])) {
            var uwl2 = roundFloat(subGroupObj.uwl2, settings.decimalPlaces);
            settings.inputs.UWL2[sgNum] = uwl2;
            chartApi.get(seriesIds.controlChart.uwl2).data[sgNum].update(uwl2);

        }
        if (isNullOrUndefined(subGroupObj.xbP2) && isNullOrUndefined(chartApi.get(seriesIds.controlChart.xbP2).data[sgNum])) {
            var xbP2 = roundFloat(subGroupObj.xbP2, settings.decimalPlaces);
            settings.inputs.xbP2[sgNum] = xbP2;
            chartApi.get(seriesIds.controlChart.xbp2).data[sgNum].update(xbP2);

        }
        if (isNullOrUndefined(subGroupObj.series2) && isNullOrUndefined(chartApi.get(seriesIds.controlChart.series2).data[sgNum])) {
            var series2 = roundFloat(subGroupObj.series2, settings.decimalPlaces);
            settings.inputs.series2[sgNum] = series2;
            chartApi.get(seriesIds.controlChart.series2).data[sgNum].update(series2);
        }

        chartApi.redraw();
    }

    function displayMeasurements() {
        var isVisible = (
            settings.showMeasurements === true &&
            (
                isXbChartType()     ||
                isMedChartType()    ||
                isMxbChartType()
            )
        );

        return isVisible;
    }

    function isXbChartType() {
        return (settings.chartType === chartTypes.xb || settings.chartType === chartTypes.xb_s || settings.chartType === chartTypes.xb_R);
    }

    function isMxbChartType() {
        return (settings.chartType === chartTypes.mxb_ms || settings.chartType === chartTypes.mxb_mR);
    }

    function isMedChartType() {
        return settings.chartType === chartTypes.med;
    }

    function prepareColoredZone(status) {
        var zone = [];
        var violationZones = [];
        var subgroupSize = settings.data && settings.data.specifications && settings.data.specifications.subgroupSize > 0 ? settings.data.specifications.subgroupSize : 1;
        var interval;
        switch (settings.chartType) {

            case chartTypes.mxb_ms:
            case chartTypes.mxb_mR:
                interval = [[0, subgroupSize - 1]];
                zone = (getColoredSeriesZone(interval, siemensColors.SiemensBlueLight));
                break;
        }
        //Disply violated samples and values in a different color 
        if (settings.data && settings.data.subgroups) {
            var violatedSubgroupsNumber = [];
            settings.data.subgroups.forEach(function (subgroup) {
                var statuses = subgroup.statuses;
                var newstatuses = [];
                if (statuses && statuses.length > 0) {
                    for (var i = 0; i < statuses.length; i++)
                        newstatuses.push(statuses[i].category.toLowerCase());
                    if (newstatuses.indexOf(status) > -1) {
                        violatedSubgroupsNumber.push(roundFloat(subgroup.subgroupNumber));
                    }
                }
            });

        }
        interval = getSequence(violatedSubgroupsNumber)
        violationZones = getColoredSeriesZone(interval, siemensColors.violationZone);
        violationZones.forEach(function (violationZone) {
            zone.push(violationZone);

        });
        return zone;


    }

    function getDefaultyAxisUnitObj() {
        var obj = {
            text: '',
            x: 60,
            y: undefined,
            fontSize: undefined
        };
        return obj;
    }

    function getDefaultSeriesSettings() {
        var obj =
        {
            color: {
                series1: chartSeriesColors.ControlChart.xbarX,
                series2: chartSeriesColors.ControlChart.Sr,
                controlLimit: chartSeriesColors.ControlChart.ControlLimit,
                warningLimit: chartSeriesColors.ControlChart.WarningLimit,
                processMeanValue: chartSeriesColors.ControlChart.xbarProcess
            },
            tooltip: []
        };
        return obj;
    }

    function parseMovingMeanWeighting(movingMeanWeighting) {
        if (movingMeanWeighting && typeof movingMeanWeighting === 'string') {
            for (var key in movingMeanWeightings) {
                if (key.toLowerCase() === movingMeanWeighting.toLowerCase())
                    return movingMeanWeightings[key];
            }
        } else {
            console.warn("Invalid MovingMeanWeighting in parameter, valid values can be one from these -> Uniformly, Exponentially ");
            return -1;
        }
        return -1;
    }

    function getCustomRemarks(settings, index, returnTr) {
        var customTooltip = [];
        var customInfo = undefined;
        if (settings.data && settings.data.subgroups && settings.data.subgroups[index]) { // Why is it needed? customInfo is already in customInfo array
            customInfo = settings.data.subgroups[index].customInfo;
        }
        if (customInfo) {
            var xAxisObj = customInfo.filter(function (k) { return k.isRemark === true; });
            if (xAxisObj) {
                xAxisObj.forEach(function (o) {
                    customTooltip.push(o.value);
                });
            }
        }
        if (returnTr === true)
            return tooltipRow.format(getLocalizedText('annotation', settings.locale), customTooltip.join(', '));

        return getLocalizedText('annotation', settings.locale) + ' : ' + customTooltip.join(', ');
        // return tooltipRow.format(getLocalizedText('annotation', settings.locale), customTooltip.join(', '));
    }

    function findSeries(inputObject, seriesId) {
        var foundSeries = null;
        if (inputObject) {
            var foundObject = inputObject.filter(function (ser) {
                if (ser && ser.series.userOptions && ser.series.userOptions.id === seriesId)
                    return ser;
                return null;
            });

            if (foundObject && foundObject.length > 0)
                foundSeries = foundObject[0];
        }
        return foundSeries;
    }

    function exportAsImage(chartApi, options) {
        if (options && chartApi) {
            var opt =
            {
                url: undefined,
                sourceHeight: undefined,
                sourceWidth: undefined,
                type: 'image/png',
                filename: 'chart',
                fallbackToExportServer: false
            }

            if (options.filename)
                opt.filename = options.filename;

            if (options.type)
                opt.type = options.type;

            if (options.width)
                opt.sourceWidth = options.width;

            if (options.height)
                opt.sourceHeight = options.height;

            chartApi.exportChartLocal(opt, {
                chart: {
                    backgroundColor: '#FFFFFF'
                }
            });
        }
    }

    function isRelevantSpcZoneChartType() {
        var isRelevant =
            settings.chartType !== chartTypes.s &&
            settings.chartType !== chartTypes.R &&
            settings.chartType !== chartTypes.ms &&
            settings.chartType !== chartTypes.mR;

        return isRelevant;

    }

    // control chart 
    function getYaxis1MinScaling() {
        if (!isNullOrUndefined(settings.yAxisScale)) {
            if (!isNullOrUndefined(settings.yAxisScale.min))
                return settings.yAxisScale.min;
            if (!isNullOrUndefined(settings.yAxisScale.minScaling))
                return getYaxis1MinValue(settings.yAxisScale.minScaling)
        }
        return settings.series1yAxisMinValue;
    }
    function getYaxis1MaxScaling() {
        if (!isNullOrUndefined(settings.yAxisScale)) {
            if (!isNullOrUndefined(settings.yAxisScale.max))
                return settings.yAxisScale.max;
            if (!isNullOrUndefined(settings.yAxisScale.maxScaling))
                return getYaxis1MaxValue(settings.yAxisScale.maxScaling)

        }
        return settings.series1yAxisMaxValue;
    }
    function getYaxis1MinValue(scalingType) {
        var minValue = null;
        switch (scalingType) {
            case scaling.controlLimit:
                var lcl = settings.inputs.LCL1.reduce(function (a, b) {
                    return Math.min(a, b);
                });
                var ucl = settings.inputs.UCL1.reduce(function (a, b) {
                    return Math.min(a, b);
                });
                minValue = Math.min(lcl, ucl);
                break;
            case scaling.toleranceLimit:
                var ltl = settings.inputs.LTL1.reduce(function (a, b) {
                    return Math.min(a, b);
                });
                var utl = settings.inputs.UTL1.reduce(function (a, b) {
                    return Math.min(a, b);
                });
                minValue = Math.min(lcl, ucl);
                break;

        }
        return minValue;
    }
    function getYaxis1MaxValue(scalingType) {
        var maxValue = null;
        switch (scalingType) {
            case scaling.controlLimit:
                var lcl = settings.inputs.LCL1.reduce(function (a, b) {
                    return Math.max(a, b);
                });
                var ucl = settings.inputs.UCL1.reduce(function (a, b) {
                    return Math.max(a, b);
                });
                maxValue = Math.max(lcl, ucl);
                break;
            case scaling.toleranceLimit:
                var ltl = settings.inputs.LTL1.reduce(function (a, b) {
                    return Math.max(a, b);
                });
                var utl = settings.inputs.UTL1.reduce(function (a, b) {
                    return Math.max(a, b);
                });
                maxValue = Math.max(ltl, utl);
                break;

        }
        return maxValue;
    }
    // control chart 
    function getYaxis2MinScaling() {
        if (!isNullOrUndefined(settings.yAxisScale)) {
            if (!isNullOrUndefined(settings.yAxisScale.secondMin))
                return settings.yAxisScale.secondMin;
            if (!isNullOrUndefined(settings.yAxisScale.secondMinScaling))
                return getYaxis2MinValue(settings.yAxisScale.secondMinScaling)
        }
        return settings.series2yAxisMinValue;
    }
    function getYaxis2MaxScaling() {
        if (!isNullOrUndefined(settings.yAxisScale)) {
            if (!isNullOrUndefined(settings.yAxisScale.secondMax))
                return settings.yAxisScale.secondMax;
            if (!isNullOrUndefined(settings.yAxisScale.secondMaxScaling))
                return getYaxis2MaxValue(settings.yAxisScale.secondMaxScaling)

        }
        return settings.series2yAxisMaxValue;
    }
    function getYaxis2MinValue(scalingType) {
        var minValue = null;
        if (scalingType == scaling.controlLimit) {
            var lcl = settings.inputs.LCL2.reduce(function (a, b) {
                return Math.min(a, b);
            });
            var ucl = settings.inputs.UCL2.reduce(function (a, b) {
                return Math.min(a, b);
            });
            minValue = Math.min(lcl, ucl);
        }
        return minValue;
    }
    function getYaxis2MaxValue(scalingType) {
        var maxValue = null;
        if (scalingType == scaling.controlLimit) {
            var lcl = settings.inputs.LCL2.reduce(function (a, b) {
                return Math.max(a, b);
            });
            var ucl = settings.inputs.UCL2.reduce(function (a, b) {
                return Math.max(a, b);
            });
            maxValue = Math.max(lcl, ucl);
        }
        return maxValue;
    }

    function applySeriesOrder(toBeReorderd) {
        //Two approches can be done. This one is done that user can specifiy random order for any series. That series will be displyed on particular location
        //Second approach could be to show mentioned series in settings in a particular order and not mentioned will be displyed at the end (note done). 
        //TODO: series_id order can be taken from param and based on this order, the series will be shown.
        var seriesOrder = settings.series.tooltip || [];

        for (var i = 0; i < seriesOrder.length; i++) {
            var oldElemAtIndex = undefined,
                tooltipSetting = seriesOrder[i];

            toBeReorderd.some(function (a, index) {
                var seriesId = $(a).attr('series_id');
                if (seriesId === tooltipSetting.seriesId && seriesId !== 'customInfo' && seriesId !== 'violation') {
                    oldElemAtIndex = index;
                    return true;
                }
            });
            // GUPTA send parameter as string, therefore compare to false string
            if (!isNullOrUndefined(tooltipSetting.showInTooltip) && tooltipSetting.showInTooltip.toString() === "false" && oldElemAtIndex > -1) { // just compare to false and not with undefined and null
                toBeReorderd[oldElemAtIndex] = "";
            } else
                if (tooltipSetting.order < toBeReorderd.length - 1 && oldElemAtIndex > -1) { // Table tag is already added, so length - 1
                    var temp = toBeReorderd[tooltipSetting.order];
                    toBeReorderd[tooltipSetting.order] = toBeReorderd[oldElemAtIndex];
                    toBeReorderd[oldElemAtIndex] = temp;
                }
        }

        var customInfoIndex = undefined;

        toBeReorderd.some(function (a, index) {
            var seriesId = $(a).attr('series_id');
            if (seriesId === "customInfo") {
                customInfoIndex = index;
                return true;
            }
        });

        if (customInfoIndex > -1) {
            toBeReorderd.splice(customInfoIndex, 0, tooltipHr);
            customInfoIndex = undefined;
        }

        toBeReorderd.some(function (a, index) {
            var seriesId = $(a).attr('series_id');
            if (seriesId === "violation") {
                customInfoIndex = index;
                return true;
            }
        });

        if (customInfoIndex > -1) {
            toBeReorderd.splice(customInfoIndex, 0, tooltipHr);
            customInfoIndex = undefined;
        }

        toBeReorderd.push(tooltipTableEnd);

        return toBeReorderd;
    }

    function prepareMeasurementsSeries(input) {
        var subgroupSize = settings.data && settings.data.specifications && settings.data.specifications.subgroupSize > 0 ? settings.data.specifications.subgroupSize : 1;

        if (isXbChartType() || isMedChartType()) {
            $.each(settings.data.measurements, function (i, measurement) {
                input.measurements.push([parseInt(i / subgroupSize), measurement.measuredValue]);
            });
        } else if (isMxbChartType()) {
            for (var i = 0; i < settings.data.measurements.length; i++) {
                for (var j = 0; j < subgroupSize && settings.data.measurements[i + j] && input.measurements.length < (settings.data.subgroups.length - 1) * subgroupSize; j++) {
                    input.measurements.push([i + subgroupSize - 1, settings.data.measurements[i + j].measuredValue]);
                }
            }
        }
    }



};

/**
 * @description Provide methods for drawing Histogram chart .
 *
 * @namespace HistogramChart
 * */
/**
 * @description A new Histogram chart  can be drawn using this method.
 * @memberof HistogramChart
 * @function histogramChart
 * @requires highstock.js
 * @requires jQuery.js
 * @param {HistogramChart.options} options - The chart options parameter
 * @tutorial CreateHistogramChart
 * @example
    var $chart = $('#HistogramS').histogramChart({
                    locale: switchlocale || 'en',
                    decimalPlaces: 4,
                    height: '60%',
                    width: undefined,
                    chartType: 'hist_S',
                    xAxisTitle:'(mm)',
                    data: data
                });
 */
/**
* @memberof  HistogramChart
* @typedef  {object} options
* @property {string}  [locale=en]  User locale to display label and violations in a specific language
* @property {HistogramChart.data} data - JSON Array with sample count, percentage and bins border
* @property {string} [chartTitle= 'HISTOGRAM'] - The Chart title
* @property {string} [xAxisTitle] - The xAxis Title
* @property {string} [yAxisTitle= '%'] - The yAxis Title
* @property {number}  [decimalPlaces=4] - The data of chart will be round based on number of decimal places.
* @property {number}  [height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
*                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
* @property {number}  [width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
* @property {HistogramChart.onChartLoaded} [onChartLoaded=null] - A callback can be register which will be invoked after chart is data is loaded and it is going to render. 
* @property {chartTypes} chartType - Enumeration to specifies which chart type should be used @see {@link chartTypes}
* @property {HistogramChart.legendSettings} legendSettings - Settings for position, layout and visibility of the chart legend
* @property {boolean}  [animation=false] -Enable or disable the initial animation
* @property {HistogramChart.titleSettings} [options.titleSettings]  - Provides options to position & rotate the chart title
* @property {HistogramChart.fontSize} [fontSize]-Option to set Fontsize of label & title.
* @property {number} [xAxisLabelsRotation] Rotation of the labels in degrees. When undefined, the engine will try to hide/autorotate lables to avoid overlapping.
* @property {HistogramChart.additionalLines} [options.additionalLines] - Provides additional lines configurations
*/
/**
*@typedef {Object} additionalLines
*@memberof HistogramChart
*@property {Object} firstChoiceLimit
*@property {string} firstChoiceLimit.lowerlabel - The label for the lower limit of the first choice.
*@property {string} firstChoiceLimit.upperlabel - The label for the upper limit of the first choice.
*@property {string} firstChoiceLimit.color - The color of the first choice limit line.
*@property {string} firstChoiceLimit.dashStyle - The dash style of the first choice limit line possible values <Solid,ShortDash,ShortDot,ShortDashDot,ShortDashDotDot,Dot,Dash,LongDash,DashDot,LongDashDot,LongDashDotDot>.
*@property {number} firstChoiceLimit.labelPosition - The vertical position of the label for the first choice limit line.
*@property {Object} secondChoiceLimit
*@property {string} secondChoiceLimit.lowerlabel - The label for the lower limit of the second choice.
*@property {string} secondChoiceLimit.upperlabel - The label for the upper limit of the second choice.
*@property {string} secondChoiceLimit.color - The color of the second choice limit line.
*@property {string} secondChoiceLimit.dashStyle - The dash style of the first choice limit line possible values <Solid,ShortDash,ShortDot,ShortDashDot,ShortDashDotDot,Dot,Dash,LongDash,DashDot,LongDashDot,LongDashDotDot>.
*@property {number} secondChoiceLimit.labelPosition - The vertical position of the label for the second choice limit line.
*@property {Object} thirdChoiceLimit
*@property {string} thirdChoiceLimit.lowerlabel - The label for the lower limit of the third choice.
*@property {string} thirdChoiceLimit.upperlabel - The label for the upper limit of the third choice.
*@property {string} thirdChoiceLimit.color - The color of the third choice limit line.
*@property {string} thirdChoiceLimit.dashStyle - The dash style of the first choice limit line possible values <Solid,ShortDash,ShortDot,ShortDashDot,ShortDashDotDot,Dot,Dash,LongDash,DashDot,LongDashDot,LongDashDotDot>.
*@property {number} thirdChoiceLimit.labelPosition - The vertical position of the label for the third choice limit line.
*/
/**
 * @typedef{object} legendSettings
 * @memberof HistogramChart
 * @property {boolean} [legendSettings.enabled = true] - show or hide the legend.
 * @property {string} [legendSettings.align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [legendSettings.verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [legendSettings.layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [legendSettings.x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [legendSettings.Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */
/**
 * @description The titleSettings object is to determine the position and rotation of the chart title
 * @typedef {object} titleSettings
 * @memberof HistogramChart
 * @property {string} [align = undefined] - The horizontal alignment of the title. Can be one of "left", "center" and "right".
 * @property {number} [rotate = undefined] - The rotation of the text in degrees. 0 is horizontal, 270 is vertical reading from bottom to top.
 * @property {number} [x = undefind] - The x position of the title relative to the alignment
 * @property {number} [y = undefind] - The y position of the title relative to the alignment
 * @example
 *titleSettings:{
 *  align: 'left',
 *  rotate: '90',
 *  x: '35',
 *  y:'207'
 *},
 */
/**
* @description The data array of the Histogram
* @typedef {object} data
* @memberof HistogramChart
* @property {number} count Number of samples that lies in the bin range
* @property {number} percentage Percentage of the sample that lies in the bin range
* @property {number} lowerBorder The Border of the bin range
* @example
*{
*	"result": {
*		"histogramClassesByS": [
*			{
*				"count": 0,
*				"percentage": 0.0,
*				"lowerBorder": 19.96251030353614
*			},
*			{
*				"count": 5,
*				"percentage": 1.0,
*				"lowerBorder": 19.96897514914596
*			},
*			{
*				"count": 12,
*				"percentage": 2.4,
*				"lowerBorder": 19.975439994755786
*			},
*			{
*				"count": 26,
*				"percentage": 5.2,
*				"lowerBorder": 19.98190484036561
*			},
*			{
*				"count": 34,
*				"percentage": 6.800000000000001,
*				"lowerBorder": 19.98836968597543
*			},
*			{
*				"count": 102,
*				"percentage": 20.4,
*				"lowerBorder": 19.994834531585256
*			},
*			{
*				"count": 126,
*				"percentage": 25.2,
*				"lowerBorder": 20.001299377195079
*			},
*			{
*				"count": 106,
*				"percentage": 21.2,
*				"lowerBorder": 20.0077642228049
*			},
*			{
*				"count": 49,
*				"percentage": 9.8,
*				"lowerBorder": 20.014229068414726
*			},
*			{
*				"count": 24,
*				"percentage": 4.8,
*				"lowerBorder": 20.020693914024549
*			},
*			{
*				"count": 3,
*				"percentage": 0.6,
*				"lowerBorder": 20.02715875963437
*			},
*			{
*				"count": 5,
*				"percentage": 1.0,
*				"lowerBorder": 20.033623605244196
*			},
*			{
*				"count": 3,
*				"percentage": 0.6,
*				"lowerBorder": 20.040088450854019
*			},
*			{
*				"count": 0,
*				"percentage": 0.0,
*				"lowerBorder": 20.04655329646384
*			}
*		],
*		"histogramClassesByTolerances": [
*			{
*				"count": 2,
*				"percentage": 0.4,
*				"lowerBorder": 19.952727272727274
*			},
*			{
*				"count": 1,
*				"percentage": 0.2,
*				"lowerBorder": 19.96
*			},
*			{
*				"count": 5,
*				"percentage": 1.0,
*				"lowerBorder": 19.96727272727273
*			},
*			{
*				"count": 12,
*				"percentage": 2.4,
*				"lowerBorder": 19.974545454545458
*			},
*			{
*				"count": 30,
*				"percentage": 6.0,
*				"lowerBorder": 19.981818181818185
*			},
*			{
*				"count": 61,
*				"percentage": 12.2,
*				"lowerBorder": 19.989090909090913
*			},
*			{
*				"count": 109,
*				"percentage": 21.8,
*				"lowerBorder": 19.99636363636364
*			},
*			{
*				"count": 136,
*				"percentage": 27.200000000000004,
*				"lowerBorder": 20.003636363636369
*			},
*			{
*				"count": 92,
*				"percentage": 18.4,
*				"lowerBorder": 20.010909090909096
*			},
*			{
*				"count": 34,
*				"percentage": 6.800000000000001,
*				"lowerBorder": 20.018181818181824
*			},
*			{
*				"count": 8,
*				"percentage": 1.6,
*				"lowerBorder": 20.02545454545455
*			},
*			{
*				"count": 5,
*				"percentage": 1.0,
*				"lowerBorder": 20.03272727272728
*			},
*			{
*				"count": 3,
*				"percentage": 0.6,
*				"lowerBorder": 20.040000000000008
*			}
*		],
*		"listOfCalculations": [
*			{
*				"inStandardDeviationRange": 312.0,
*				"inStandardDeviationRangePercentage": 62.4,
*				"countOfGreaterStandardDeviation": 0.0,
*				"countOfGreaterStandardDeviationPercentage": 0.0,
*				"countOfGreaterNegativeStandardDeviation": 0.0,
*				"countOfGreaterNegativeStandardDeviationPercentage": 0.0,
*				"countOfGreaterPositiveStandardDeviation": 0.0,
*				"countOfGreaterPositiveStandardDeviationPercentage": 0.0,
*				"inSigmaRange": 312.0,
*				"inSigmaRangePercentage": 62.4,
*				"lowerStandardDeviation": 19.99564263728649,
*				"upperStandardDeviation": 20.013420962713508
*			},
*			{
*				"inStandardDeviationRange": 124.0,
*				"inStandardDeviationRangePercentage": 24.8,
*				"countOfGreaterStandardDeviation": 0.0,
*				"countOfGreaterStandardDeviationPercentage": 0.0,
*				"countOfGreaterNegativeStandardDeviation": 0.0,
*				"countOfGreaterNegativeStandardDeviationPercentage": 0.0,
*				"countOfGreaterPositiveStandardDeviation": 0.0,
*				"countOfGreaterPositiveStandardDeviationPercentage": 0.0,
*				"inSigmaRange": 436.0,
*				"inSigmaRangePercentage": 87.2,
*				"lowerStandardDeviation": 19.986753474572983,
*				"upperStandardDeviation": 20.022310125427017
*			},
*			{
*				"inStandardDeviationRange": 42.0,
*				"inStandardDeviationRangePercentage": 8.4,
*				"countOfGreaterStandardDeviation": 22.0,
*				"countOfGreaterStandardDeviationPercentage": 4.3999999999999999,
*				"countOfGreaterNegativeStandardDeviation": 12.0,
*				"countOfGreaterNegativeStandardDeviationPercentage": 2.4,
*				"countOfGreaterPositiveStandardDeviation": 10.0,
*				"countOfGreaterPositiveStandardDeviationPercentage": 2.0,
*				"inSigmaRange": 478.0,
*				"inSigmaRangePercentage": 95.60000000000001,
*				"lowerStandardDeviation": 19.97786431185947,
*				"upperStandardDeviation": 20.031199288140529
*			},
*			{
*				"inStandardDeviationRange": 14.0,
*				"inStandardDeviationRangePercentage": 2.8000000000000004,
*				"countOfGreaterStandardDeviation": 8.0,
*				"countOfGreaterStandardDeviationPercentage": 1.6,
*				"countOfGreaterNegativeStandardDeviation": 3.0,
*				"countOfGreaterNegativeStandardDeviationPercentage": 0.6,
*				"countOfGreaterPositiveStandardDeviation": 5.0,
*				"countOfGreaterPositiveStandardDeviationPercentage": 1.0,
*				"inSigmaRange": 492.0,
*				"inSigmaRangePercentage": 98.4,
*				"lowerStandardDeviation": 19.96897514914596,
*				"upperStandardDeviation": 20.040088450854037
*			},
*			{
*				"inStandardDeviationRange": 4.0,
*				"inStandardDeviationRangePercentage": 0.8,
*				"countOfGreaterStandardDeviation": 4.0,
*				"countOfGreaterStandardDeviationPercentage": 0.8,
*				"countOfGreaterNegativeStandardDeviation": 2.0,
*				"countOfGreaterNegativeStandardDeviationPercentage": 0.4,
*				"countOfGreaterPositiveStandardDeviation": 2.0,
*				"countOfGreaterPositiveStandardDeviationPercentage": 0.4,
*				"inSigmaRange": 496.0,
*				"inSigmaRangePercentage": 99.2,
*				"lowerStandardDeviation": 19.960085986432455,
*				"upperStandardDeviation": 20.048977613567545
*			},
*			{
*				"inStandardDeviationRange": 0.0,
*				"inStandardDeviationRangePercentage": 0.0,
*				"countOfGreaterStandardDeviation": 0.0,
*				"countOfGreaterStandardDeviationPercentage": 0.0,
*				"countOfGreaterNegativeStandardDeviation": 0.0,
*				"countOfGreaterNegativeStandardDeviationPercentage": 0.0,
*				"countOfGreaterPositiveStandardDeviation": 0.0,
*				"countOfGreaterPositiveStandardDeviationPercentage": 0.0,
*				"inSigmaRange": 496.0,
*				"inSigmaRangePercentage": 99.2,
*				"lowerStandardDeviation": 19.951196823718946,
*				"upperStandardDeviation": 20.057866776281054
*			},
*			{
*				"inStandardDeviationRange": 0.0,
*				"inStandardDeviationRangePercentage": 0.0,
*				"countOfGreaterStandardDeviation": 0.0,
*				"countOfGreaterStandardDeviationPercentage": 0.0,
*				"countOfGreaterNegativeStandardDeviation": 0.0,
*				"countOfGreaterNegativeStandardDeviationPercentage": 0.0,
*				"countOfGreaterPositiveStandardDeviation": 0.0,
*				"countOfGreaterPositiveStandardDeviationPercentage": 0.0,
*				"inSigmaRange": 0.0,
*				"inSigmaRangePercentage": 0.0,
*				"lowerStandardDeviation": 19.942307661005438,
*				"upperStandardDeviation": 20.06675593899456
*			},
*			{
*				"inStandardDeviationRange": 0.0,
*				"inStandardDeviationRangePercentage": 0.0,
*				"countOfGreaterStandardDeviation": 0.0,
*				"countOfGreaterStandardDeviationPercentage": 0.0,
*				"countOfGreaterNegativeStandardDeviation": 0.0,
*				"countOfGreaterNegativeStandardDeviationPercentage": 0.0,
*				"countOfGreaterPositiveStandardDeviation": 0.0,
*				"countOfGreaterPositiveStandardDeviationPercentage": 0.0,
*				"inSigmaRange": 0.0,
*				"inSigmaRangePercentage": 0.0,
*				"lowerStandardDeviation": 19.93341849829193,
*				"upperStandardDeviation": 20.07564510170807
*			},
*			{
*				"inStandardDeviationRange": 0.0,
*				"inStandardDeviationRangePercentage": 0.0,
*				"countOfGreaterStandardDeviation": 0.0,
*				"countOfGreaterStandardDeviationPercentage": 0.0,
*				"countOfGreaterNegativeStandardDeviation": 0.0,
*				"countOfGreaterNegativeStandardDeviationPercentage": 0.0,
*				"countOfGreaterPositiveStandardDeviation": 0.0,
*				"countOfGreaterPositiveStandardDeviationPercentage": 0.0,
*				"inSigmaRange": 0.0,
*				"inSigmaRangePercentage": 0.0,
*				"lowerStandardDeviation": 19.924529335578418,
*				"upperStandardDeviation": 20.08453426442158
*			},
*			{
*				"inStandardDeviationRange": 0.0,
*				"inStandardDeviationRangePercentage": 0.0,
*				"countOfGreaterStandardDeviation": 0.0,
*				"countOfGreaterStandardDeviationPercentage": 0.0,
*				"countOfGreaterNegativeStandardDeviation": 0.0,
*				"countOfGreaterNegativeStandardDeviationPercentage": 0.0,
*				"countOfGreaterPositiveStandardDeviation": 0.0,
*				"countOfGreaterPositiveStandardDeviationPercentage": 0.0,
*				"inSigmaRange": 0.0,
*				"inSigmaRangePercentage": 0.0,
*				"lowerStandardDeviation": 19.915640172864909,
*				"upperStandardDeviation": 20.09342342713509
*			}
*		],
*		"distributionValuesModels": null,
*		"processValues": {
*			"calculatedSb": 0.008355812950698488,
*			"calculatedRb": 0.02052000000000014,
*			"calculatedXbb": 20.0045318,
*			"calculatedMin": 19.955,
*			"calculatedMax": 20.055,
*			"cp": 1.4999537935187222,
*			"cpk": 1.3300165284820618,
*			"processIsUnderControl": false,
*			"processIsCapable": false,
*			"countOfValidValues": 500,
*			"countOfValidValuesInPercent": 100.0,
*			"sigmaEstimated": 0.00888916271350903,
*			"countOfValuesLessThanLowerTolerance": 0.0,
*			"countOfValuesLargerThanUpperTolerance": 0.0,
*			"countOfValuesLessThanLowerToleranceInPercent": 0.0,
*			"countOfValuesLargerThanUpperToleranceInPercent": 0.0,
*			"range": 0.10000000000000142,
*			"maxXb": 20.0408,
*			"minXb": 19.9704,
*			"probabilityOfValuesLargerThanUpperToleranceInPercent": 0.0,
*			"probabilityOfValuesLessThanLowerToleranceInPercent": 0.0,
*			"meanModification": 0.0,
*			"p99": 20.040088450854037,
*			"p0_13": 19.96897514914596
*		},
*	},
*	"specifications": {
*		"subgroupSize": 5,
*		"distributionType": null,
*		"controlChartType": "xb_s",
*		"limitationType": null,
*		"averageMovingMeanType": null,
*		"evaluationType": null,
*		"capabilityStudyType": null,
*		"limit": null,
*		"currentNominalValue": null,
*		"currentUpperToleranceLimitAbs": 20.04,
*		"currentLowerToleranceLimitAbs": 19.96,
*		"defectRate": null,
*		"probabilityOfAction": null,
*		"cL_L_Percent": null,
*		"cL_U_Percent": null,
*		"wL_L_Percent": null,
*		"wL_U_Percent": null,
*		"confidenceInterval": 4,
*		"controlLimits": {
*			"currentUpperControlLimit1Abs": null,
*			"currentLowerControlLimit1Abs": null,
*			"currentUpperWarnLimit1Abs": null,
*			"currentLowerWarnLimit1Abs": null,
*			"currentUpperControlLimit2Abs": null,
*			"currentLowerControlLimit2Abs": null,
*			"currentUpperWarnLimit2Abs": null,
*			"currentLowerWarnLimit2Abs": null,
*			"currentProcessMeanValue1": null,
*			"currentProcessMeanValue2": null
*		}
*	}
*}
*
* */
/**
 * @typedef {object} fontSize
 * @memberof HistogramChart
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */
$.fn.histogramChart = function (options) {
    setDefaults();
    var settings = {},
        tooltipRow = '<tr style="text-align: left; padding: 5px;"><td>{0}: {1} ({2})</td></tr>',
        tooltipHr = '<tr><td colspan="2;" style = "padding: 3px 4px 3px 4px;"><hr style="margin-top: 0; padding: 0; margin-bottom: 0;"/></td></tr>',
        tooltipTableStart = '<table style="border-spacing: 0px;"><tbody>',
        tooltipTableEnd = '</tbody></table>';

    if (options && typeof options === 'object') {
        settings = $.extend({
            locale: 'en',
            data: {},
            chartTitle: getLocalizedText('Hist_ChartTitle', options.locale),
            xAxisTitle: '',
            yAxisTitle: getLocalizedText('Hist_yAxisTitle', options.locale),
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            chartType: undefined,
            onChartLoaded: undefined,
            legendSettings: {
                enabled: true,
                align: 'right',
                verticalAlign: 'top',
                layout: 'vertical',
                x: -50,
                y: 250
            },
            titleSettings: undefined,
            animation: false,
            xAxisLabelsRotation: undefined,
            additionalLines: {
                firstChoiceLimit: {
                    lowerlabel: '',
                    upperlabel: '',
                    color: 'black',
                    dashStyle: 'solid',
                    labelPosition:-11
                },
                secondChoiceLimit: {
                    lowerlabel: '',
                    upperlabel: '',
                    color: 'red',
                    dashStyle: 'solid',
                    labelPosition: -11
                },
                thirdChoiceLimit: {
                    lowerlabel: '',
                    upperlabel: '',
                    color: 'red',
                    dashStyle: 'solid',
                    labelPosition: -11
                }
            }
        }, options);
    } else if (typeof options === 'string') { // Method call
        settings = $(this).data('settings') || {};
    }  // restore the settings object from prev settings
    var $this = this,
        _args = arguments,
        HistogramChart = {
            init: function () {
                var containerId = $this.attr('id');
                var _chart = Highcharts.chart(containerId, createSVHistogramOptionValue(settings));
                $this.data('chartApi', _chart);
                $this.data('settings', settings);
            },

            /**
            * @function addPoint
            * @description Add a new point to  histograms. The histogram chart must be drawn before.
            * @param {number} newPoint The value of the point that being added.
            * @memberof HistogramChart
            * @example
            * $('#chart').chart('addPoint', 14.068);
            * // This will add a new point to the measurements and then calculate the new classes.
            */
            addPoint: function (newPoint) {
                var chartApi = $this.data('chartApi');
                var currentClasses = chartApi.get(seriesIds.HistogramChart.Classes).data;
                if (currentClasses) {
                    //console.log(currentClasses);

                    // var i;
                    for (var i = 0; i < currentClasses.length -1; i++) {
                        if (currentClasses[i].x <= newPoint && currentClasses[i + 1].x > newPoint) {
                            settings.dataCount++;
                            settings.data.result.histogramClassesByTolerances[i].count += 1;
                            break;
                        }
                    }
                    for (var i = 0; i < settings.data.result.histogramClassesByTolerances.length - 1; i++) {
                        var newProcentage = settings.data.result.histogramClassesByTolerances[i].count / settings.dataCount * 100;
                        settings.data.result.histogramClassesByTolerances[i].percentage = newProcentage;
                        currentClasses[i].y = newProcentage;
                    }
                    chartApi.get(seriesIds.HistogramChart.Classes).setData(currentClasses);
                    chartApi.redraw();
                    $this.data('settings', settings);
                }
            },

            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof HistogramChart
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered.
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#histogramChart').histogramChart('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64',
            *                                           width : 1800,
            *                                           height: 600,
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        };
    return this.each(function () {
        if (HistogramChart[options]) {
            return HistogramChart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            HistogramChart.init();
        }
    });
    //Create Histogram Option Values
    function createSVHistogramOptionValue(settings, dataCount) {
        
        var data = [];
        var categories = [];
        var plotLines = [];
        var histogramPoints = [];
        var bellCurve = [];
        var maxValue = 0;
        var dataCount = 0;
        // var labelCounter = 0;
        var siemens_tooltip = $.extend({}, siemensTooltip);

        if (settings.data.result) {
            var diff = 0;
            
            switch (parseChartType(settings.chartType)) {
                case chartTypes.hist_S:
                    data = prepareData(settings.data.result.histogramClassesByS);
                    if (data && data.length > 1) {
                        var classWidth = data[1][0] - data[0][0];
                        diff = classWidth / 2;
                    }
                    bellCurve = prepareBellCurve(settings.data.result.bellCurvePointsS, diff);
                    break;
                case chartTypes.hist_T:
                    data = prepareData(settings.data.result.histogramClassesByTolerances);
                    if (data && data.length > 1) {
                        var classWidth = data[1][0] - data[0][0];
                        diff = classWidth / 2;
                    }
                    bellCurve = prepareBellCurve(settings.data.result.bellCurvePointsTolerances, diff);
                    break;
            }
        }
        if (data && data.length > 0) {
            var classWidth = data[1][0] - data[0][0];
            var diff = classWidth / 2;


            if (data) {
                $.each(data, function (i, item) {
                    var centerPoint = item[0] - diff;
                    histogramPoints.push([item[0], item[1]]);
                    categories.push(centerPoint);
                    dataCount+= item[1];
                });
                settings.dataCount = dataCount;
                
            }
            //declare all lines as null
            var currentLowerToleranceLimitAbs,
                firstChoiceLowerLimit,
                firstChoiceUpperLimit,
                secondChoiceLowerLimit,
                secondChoiceUpperLimit,
                thirdChoiceLowerLimit,
                thirdChoiceUpperLimit,
                currentUpperToleranceLimitAbs,
                confidenceInterval,
                calculatedXbb,
                plusD,
                minusD,
                confidenceIntervalString;

            var lowerBorder = Math.min.apply(Math, histogramPoints.map(function (o) {
                return o[0];
            }));
            var upperBorder = Math.max.apply(Math, histogramPoints.map(function (o) {
                return o[0];
            }));
            if (settings.data.additionalLines) {
                firstChoiceLowerLimit = normalizePlotLine(settings.data.additionalLines.firstChoiceLowerLimit, diff, lowerBorder, upperBorder);
                firstChoiceUpperLimit = normalizePlotLine(settings.data.additionalLines.firstChoiceUpperLimit, diff, lowerBorder, upperBorder);
                secondChoiceLowerLimit = normalizePlotLine(settings.data.additionalLines.secondChoiceLowerLimit, diff, lowerBorder, upperBorder);
                secondChoiceUpperLimit = normalizePlotLine(settings.data.additionalLines.secondChoiceUpperLimit, diff, lowerBorder, upperBorder);
                thirdChoiceLowerLimit = normalizePlotLine(settings.data.additionalLines.thirdChoiceLowerLimit, diff, lowerBorder, upperBorder);
                thirdChoiceUpperLimit = normalizePlotLine(settings.data.additionalLines.thirdChoiceUpperLimit, diff, lowerBorder, upperBorder);


            }
            //Since we substract diff from category sothat the category are drown exactly at the beginning of histogram column
            // we need to do the same for all plotted lines so that they are drwan exactly at the original values 
            //Highchrts drw the column in the middle of the x value 
            // we correct the displaed text in the lable formatter in x-Axis to display the original x values 
            if (settings.data.specifications) {
                currentLowerToleranceLimitAbs = normalizePlotLine(roundFloat(settings.data.specifications.currentLowerToleranceLimitAbs, settings.decimalPlaces, true), diff, lowerBorder, upperBorder);
                currentUpperToleranceLimitAbs = normalizePlotLine(roundFloat(settings.data.specifications.currentUpperToleranceLimitAbs, settings.decimalPlaces, true), diff, lowerBorder, upperBorder);
               
                confidenceInterval = roundFloat(settings.data.specifications.confidenceInterval || null);
                confidenceIntervalString = confidenceInterval == null ? '' : confidenceInterval.toString() + getLocalizedText('HIST_Confidance_Interval', settings.locale);
            }
            if (settings.data.result) {
                if (confidenceInterval && settings.data.result.listOfCalculations) {
                    plusD = normalizePlotLine(roundFloat(settings.data.result.listOfCalculations[confidenceInterval - 1].upperStandardDeviation, settings.decimalPlaces, true), diff, lowerBorder, upperBorder);
                    minusD = normalizePlotLine(roundFloat(settings.data.result.listOfCalculations[confidenceInterval - 1].lowerStandardDeviation, settings.decimalPlaces, true), diff, lowerBorder, upperBorder);
                    minusD = roundFloat(settings.data.result.listOfCalculations[confidenceInterval - 1].lowerStandardDeviation || null)-diff;
                }
                if (settings.data.result.processValues) {

                    calculatedXbb = normalizePlotLine(settings.data.result.processValues.calculatedXbb, diff, lowerBorder, upperBorder);
                    
                }
            }

            var plotlinesValues = [];
            plotlinesValues.push(currentLowerToleranceLimitAbs,
                currentUpperToleranceLimitAbs,
                calculatedXbb,
                plusD,
                minusD,
                firstChoiceLowerLimit,
                firstChoiceUpperLimit,
                secondChoiceLowerLimit,
                secondChoiceUpperLimit,
                thirdChoiceLowerLimit,
                thirdChoiceUpperLimit);
            var maxPlotLinePoint = Math.max.apply(Math, plotlinesValues.reduce(function (result, plotlinesValue) {
                if (plotlinesValue) {
                    result.push(plotlinesValue);
                }
                return result;
            }, []));

            var minPlotLinePoint = Math.min.apply(Math, plotlinesValues.reduce(function (result, plotlinesValue) {
                if (plotlinesValue) {
                    result.push(plotlinesValue);
                }
                return result;
            }, []));

            plotLines.push(createPlotLineObject(currentLowerToleranceLimitAbs,'red', getLocalizedText('Hist_currentLowerToleranceLimitAbs', settings.locale), -11));
            plotLines.push(createPlotLineObject(currentUpperToleranceLimitAbs, chartSeriesColors.Histogram.ToleranceLimit, getLocalizedText('Hist_currentUpperToleranceLimitAbs', settings.locale), -11));
            plotLines.push(createPlotLineObject(firstChoiceLowerLimit, settings.additionalLines.firstChoiceLimit.color, settings.additionalLines.firstChoiceLimit.lowerlabel, settings.additionalLines.firstChoiceLimit.labelPosition, settings.additionalLines.firstChoiceLimit.dashStyle));
            plotLines.push(createPlotLineObject(firstChoiceUpperLimit, settings.additionalLines.firstChoiceLimit.color, settings.additionalLines.firstChoiceLimit.upperlabel, settings.additionalLines.firstChoiceLimit.labelPosition, settings.additionalLines.firstChoiceLimit.dashStyle));
            plotLines.push(createPlotLineObject(secondChoiceLowerLimit, settings.additionalLines.secondChoiceLimit.color, settings.additionalLines.secondChoiceLimit.lowerlabel, settings.additionalLines.secondChoiceLimit.labelPosition, settings.additionalLines.secondChoiceLimit.dashStyle));
            plotLines.push(createPlotLineObject(secondChoiceUpperLimit, settings.additionalLines.secondChoiceLimit.color, settings.additionalLines.secondChoiceLimit.upperlabel, settings.additionalLines.secondChoiceLimit.labelPosition, settings.additionalLines.secondChoiceLimit.dashStyle));
            plotLines.push(createPlotLineObject(thirdChoiceLowerLimit, settings.additionalLines.thirdChoiceLimit.color, settings.additionalLines.thirdChoiceLimit.lowerlabel, settings.additionalLines.thirdChoiceLimit.labelPosition, settings.additionalLines.thirdChoiceLimit.dashStyle));
            plotLines.push(createPlotLineObject(thirdChoiceUpperLimit, settings.additionalLines.thirdChoiceLimit.color, settings.additionalLines.thirdChoiceLimit.upperlabel, settings.additionalLines.thirdChoiceLimit.labelPosition, settings.additionalLines.thirdChoiceLimit.dashStyle));
            if (!settings.data.result.igcCalculation) {
                plotLines.push(createPlotLineObject(calculatedXbb, chartSeriesColors.Histogram.Xbb, getLocalizedText('Hist_Xbb', settings.locale), -1));
                plotLines.push(createPlotLineObject(plusD, chartSeriesColors.Histogram.StandardDeviation, '+' + confidenceIntervalString, -1));
                plotLines.push(createPlotLineObject(minusD, chartSeriesColors.Histogram.StandardDeviation, '-' + confidenceIntervalString, -1));
            }

            while ((categories[categories.length - 1] < maxPlotLinePoint + diff)) {
                categories.push(categories[categories.length - 1] + classWidth);
            }

            while ((categories[0] > minPlotLinePoint - diff)) {
                var newcat = categories[0] - classWidth;
                categories.unshift(newcat);
            }
            maxValue = Math.max.apply(Math, histogramPoints.map(function (o) {
                return o[1];
            }));
        }
        var optionValues = {
            chart: {
                spacingRight: 30,
                spacingTop: 35,
                marginLeft: 130,
                height: settings.height,
                width: settings.width,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                },
                events: {
                    load: onChartLoaded
                }
            },
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            credits: {
                enabled: false
            },
            title: {
                text: settings.chartTitle,
                align: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.align)) ? 'center' : settings.titleSettings.align,
                style: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.titleStyle)) ? titleStyle : settings.titleSettings.titleStyle,
            },
            tooltip: siemens_tooltip, // siemensTooltip,
            xAxis: [{
                title: {
                    text: settings.xAxisTitle,
                    style: axiesTitleStyle
                },
                tickPositions: categories,
                endOnTick: false,
                startOnTick: false,
                minPadding: 0,
                maxPadding: 0,
                min: categories.length > 0 ? categories.sort(function (a, b) {
                    return a - b;
                })[0] : 0,
                max: categories.length > 0 ? categories.sort(function (a, b) {
                    return a - b;
                })[categories.length - 1] : 0,
                plotLines: plotLines,
                gridLineColor: siemensColors.PL_BLACK_22,
                lineColor: siemensColors.PL_BLACK_22,
                labels: {
                    rotation: settings.xAxisLabelsRotation,
                    formatter: function () {

                        return roundFloat(this.value + diff, settings.decimalPlaces);
                    },
                    style: axiesStyle
                }
            }, {
                visible: false,
                linkedTo: 0,
                gridLineWidth: 1
                }],
            yAxis: {
                title: {
                    text: settings.yAxisTitle,
                    rotation: -90,
                    margin: 5,
                    align: 'middle',
                    y: 10,
                    style: axiesTitleStyle
                },
                lineWidth: 1,
                lineColor: siemensColors.PL_BLACK_22,
                alignTicks: false,
                min: 0,
                tickInterval: maxValue > 50 ? 10 : maxValue > 20 ? 5 : maxValue > 10 ? 2 : 1,
                labels: {
                    style: axiesStyle//,
                    //formatter: function () {
                    //    return histogramLabelFormatter(this.value, settings.dataCount);
                    //}
                }
            },
            plotOptions: {
                series: {
                    showInLegend: false,
                    states: {
                        inactive: {
                            opacity: 0.8
                        }
                    }
                },
                coulmn: {
                    animation: settings.animation
                }
            },
            series: [{
                id: seriesIds.HistogramChart.Classes,                
                name: seriesIds.HistogramChart.Classes,
                type: 'column',
                data: histogramPoints,
                pointPadding: -0.3287,
                color: chartSeriesColors.Histogram.Bin,
                xAxis: 0,
                pointRange: 0
               },
                {
                    id: 'bellCurve',
                    xAxis: 1,
                    data: bellCurve,
                    type: 'spline',
                    showInLegend: !settings.data.result.igcCalculation,
                    name: getLocalizedText('Hist_BellCurve', settings.locale),
                    marker: { enabled: false }

                }]
        };
        validateLegendPositioning(optionValues.legend);
        return optionValues;
    }
    //Create plot Line object to be displayed along with the Histogram Bins
    function createPlotLineObject(value, color, text, yLabel, dashStyle) {
        if (!dashStyle)
            dashStyle = 'solid'
        var label;
        label = {
            'text': text,
            'rotation': 0,
            'align': 'center',
            'x': 0,
            'y': yLabel,
            'style': {
                'fontSize': settings.fontSize.labels || axiesStyle.fontSize,
                'color': color,
                'margin': '5px'
            }
        };
        return {
            'value': value,
            'width': 2,
            'color': color,
            'label': label,
            'dashStyle': dashStyle,
            'zIndex': 4
        };
    }
    //Display ToolTip
    function displayHistoTooltip(/*tooltip-HistoGram*/) {
        var caption = getLocalizedText('Hist_tt_Count', settings.locale);
        var toolTip = [];
        var ToolTipObj = getTooltipObj(this.y);
        toolTip.push(tooltipHr);
        toolTip.push(tooltipTableStart);
        if (ToolTipObj && ToolTipObj[0]) {
            toolTip.push(tooltipRow.format(caption, ToolTipObj[0].count ? ToolTipObj[0].count : '', roundFloat(ToolTipObj[0].percentage ? ToolTipObj[0].percentage:'', settings.decimalPlaces) + '%'));
        }
        toolTip.push(tooltipTableEnd);
        return toolTip;
    }

    function getTooltipObj(y) {
        var MeasureObj = undefined;
        switch (parseChartType(settings.chartType)) {
            case chartTypes.hist_S:
                MeasureObj = settings.data.result.histogramClassesByS;
                break;
            case chartTypes.hist_T:
                MeasureObj=settings.data.result.histogramClassesByTolerances;
                break;
        }
        if (MeasureObj) {
            var obj = MeasureObj.filter(function (obj) {
                return obj.percentage === y

            });
            return obj;
        }
       

    }
    //Prepare the data to fit the Histogram in the form of 2d array [class,count]
    function prepareData(data) {
        var rowdata = new Array();
        if (!data) {
            return null;
        }
        $.each(data, function (index, item) {
            rowdata.push([roundFloat(item.lowerBorder), roundFloat(item.percentage)]);
        });
        return rowdata;
    }

    //Prepare the data for the bell curve
    function prepareBellCurve(bellCurveData, diff) {
        var rowdata = new Array();
        if (!bellCurveData) {
            return null;
        }
        $.each(bellCurveData, function (index, item) {
            rowdata.push([roundFloat(item.xValue - diff), roundFloat(item.yValue)]);
        });
        return rowdata;
    }

    function setDefaults() {
        siemensTooltip.formatter = displayHistoTooltip;

        if (typeof options === 'object') {
            if ((!options.locale || !Highcharts.uiLocale[options.locale])) {
                console.error('Locale ' + options.locale + ' not found, default English locales will be used');
                options.locale = 'en';
            }

            if (options.fontSize && options.fontSize.labels) {
                axiesTitleStyle.fontSize = options.fontSize.labels;
            }
            if (options.fontSize && options.fontSize.title) {
                titleStyle.fontSize = options.fontSize.title;
            }
            if (options.titleSettings) {
                options.titleSettings.titleStyle = $.extend({}, titleStyle);
                options.titleSettings.titleStyle.transform = 'translate(' + options.titleSettings.x + 'px, ' + options.titleSettings.y + 'px) rotate(' + options.titleSettings.rotate + 'deg)'
            }
        }
    }

    function normalizePlotLine(value, diff, lowerBorder, upperBorder) {
        if (isNullOrUndefined(value))
            return null;
        var normalizedValue = null;
        normalizedValue = value - diff;
        if (normalizedValue < lowerBorder || normalizedValue > upperBorder) {
            normalizedValue = null;
        }

        return normalizedValue;

    }
    /**
    * @description The callback which is called when histogram data is loaded and rendering started.
    * @memberof HistogramChart
    * @callback onChartLoaded
    * @param {object} e - The event object with chart API object
    * @example
    * // When a callback is registered with options in histogram
    *  $('#histogram').histogramChart(
    *                  {
    *                      locale: "en",
    *                      data: {
    *                          // the data in specified format
    *                      },
    *                      decimalPlaces: 2,
    *                      onChartLoaded : function (data) {
    *                                          console.log(data);
    *                                       }
    *                  }
    *              );
    */
    function onChartLoaded(e) {
        if (typeof settings.onChartLoaded === 'function') { // if callback is defined
            settings.onChartLoaded(e);
        }
    }
};
/**
 * @description Provide methods for single value chart .
 * @namespace SingleValueChart
 */

/**
 * @description Draw a single Value chart using the provided data from the options parameter
 * @memberof SingleValueChart
 * @function singleValueChart
 * @requires highstock.js
 * @requires jQuery.js
 * @param {SingleValueChart.options} options - The single Value chart options object.
 * @example
 * $('#svc').singleValueChart(
 *      {
 *          locale: "en",
 *          data: {
 *                  specifications : {
 *                                      subgroupSize : 2,
 *                                      controlChartType : 'xb_s',
 *                                  }
 *                  measurements : [], // Array of objects in defined format
 *                  subgroups: [],  // Array of objects in defined format
 *              },
 *           decimalPlaces: 2,
 *           xAxisCustomInfoLabel: true,
 *           hiddenSeries: [
 *              {  
 *                  "seriesId": "firstChoiceUpperLimit",
 *                  "showInChart": false,
 *                  "showInLegend": true
 *               },
 *              {
 *                  "seriesId": "secondChoiceUpperLimit",
 *                  "showInLegend": true,
 *                  "showInChart": false
 *               },
 *               {
 *                  "seriesId": "thirdChoiceUpperLimit",
 *                  "showInLegend": false,
 *                  "showInChart": false
 *               }
 *           ],
 *           onSeriesClick: function (data) {
 *                      console.log(data);
 *           },
 *           onLegendClick : function (data) {
 *                      console.log(data);
 *           }
 *        });
 *
 * @tutorial CreateSingleValueChart
 *
 */

/**
 * @description The format of data for single value chart.
 * @typedef {Array} data - The array of objects
 * @memberof SingleValueChart
 * @property {Array} subgroups - The array of objects in defined format. Please look the format in the in example below.
 * @property {Array} measurements - The array of objects in defined format.  Please look the format in the in example below.
 * @property {object} specifications - The object represent the corresponding specifications e.g. subgroupSize, controlChartType
 * @property {number} specifications.subgroupSize - The size of the subgroup.
 * @property {chartTypes} specifications.controlChartType - The type of control chart. 
 * @property {Array} [additionalLines] - optional array of more lines as decribed. The values can be overwritten by subgroups, if different values are needed.
 * @property {number[]} [additionalMeasurements] - optional number array of a additional measurements.
 * @example
 * var data = {
 *      specifications: {
 *                          subgroupSize: 5,
 *                          controlChartType : 'xb_s'                      
 *                      },
 *      subgroups: [
 *                      {
 *                          subgroupNumber: 0
 *                          calculatedS: 0.0022
 *                          calculatedR: 0.0060
 *                          calculatedXb: 19.995
 *                          calculatedMin: 19.99
 *                          calculatedMax: 19.99
 *                          statuses: null
 *                          upperToleranceLimitAbs: 20.04
 *                          lowerToleranceLimitAbs: 19.96
 *                          upperControlLimit1Abs: 20.035
 *                          lowerControlLimit1Abs: 19.965
 *                          upperWarnLimit1Abs: 20.035
 *                          lowerWarnLimit1Abs: 19.975
 *                          upperControlLimit2Abs: 0.021
 *                          lowerControlLimit2Abs: 0.0005
 *                          upperWarnLimit2Abs: 0.018
 *                          lowerWarnLimit2Abs: 0.0015
 *                          calculatedProcessMeanValue1: 20.0045
 *                          calculatedProcessMeanValue2: 0.0083
 *                          processSigma: 0.0088
 *                          initialSortNumber: 0
 *                          additionalLines : [
 *                            {
 *                              // every line is optional, if missing but there is a value in data.additionalLines, this data is uesed
 *                              firstChoiceUpperLimit: 20.01
 *		                        firstChoiceLowerLimit: 19.99
 *	                            secondChoiceUpperLimit: 20.02
 *		                        secondChoiceLowerLimit: 19.98
 *		                        thirdChoiceUpperLimit: 20.04
 *		                        thirdChoiceLowerLimit: 19.96
 *		                        upperMiddlethird: 20.1
 *		                        upperMiddlethird: 19.9
 *                              }
 *                         ],
 *                      }
 *            ],
 *      measurements : [
 *                      {
 *                          referenceID: "1"
 *                          measuredValue: 19.993
 *                          transformedMeasuredValue: 0
 *                          valueTimestamp: "0001-01-01T00:00:00"
 *                          statuses: null
 *                          sequenceID: 0,
 *                          subgroupNumber: 0
 *                      }
 *                  ],
 *    additionalLines : [
 *                      {
 *                          // every line is optional, values are displayed if there are no corresponding values in subgroups
 *                          firstChoiceUpperLimit: 20.01,
							firstChoiceLowerLimit: 19.99,
							secondChoiceUpperLimit: 20.02,
							secondChoiceLowerLimit: 19.98,
							thirdChoiceUpperLimit: 20.04,
							thirdChoiceLowerLimit: 19.96
							upperMiddlethird: 20.0
 *		                    upperMiddlethird: 19.7
 *                      }
 *                  ],
 *    additionalMeasurements: [
 *                              19.900
 *                            ]
 *  }
 *
 * // Note: The count of measurement array objects should be based on following formula
 * // if chart type is  'x_ms', 'x_mr', 'mx_ms', 'mx_mr' then size of measurements and size of subgroups array should be same.
 * // in other case the size of measurements array should be counted like this
 * // count of measurement array objects =  subgroupSize * count of subgroup array
 */

/**
 * @memberof SingleValueChart
 * @typedef  {object} options
 * @property {string} [locale=en]  User locale
 * @property {SingleValueChart.data} data - JSON Array with subgroup, measurements and specifications
 * @property {string} [chartTitle = 'Single Value Chart'] - The default value will be based on corresponding locale (if referenced).
 * @property {number} [decimalPlaces = 4] - Number of decimal places
 * @property {number} [height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number} [width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string} [xAxisTitle=null] - The x-Axis label displayed underneath the x-axies by Default it set from the localization file
 * @property {string} [yAxisTitle=null] - The x-Axis label displayed underneath the x-axies by Default it set from the localization file
 * @property {number} [yAxisMinValue=auto] - A Number specifies the minimum value of yAxis. Measurement values from this value will be shown in chart.
 * @property {number} [yAxisMaxValue=auto] - A Number specifies the maximum value of yAxis. Measurement values till this value will be shown in chart.
 * @property {SingleValueChart.seriesVisibilityObject} [hiddenSeries] - A series contain the IDs, visibility in the legend and the initial visibility state of the series.
 *              To show additional lines you must have an entry for each of this lines, because they are invisible by default. {@link SingleValueChart.seriesIds}, {@link SingleValueChart.seriesVisibilityObject}
 * @property {SingleValueChart.onLegendClick} [onLegendClick] -  The callback function which will be fired if a legend item is clicked.
 * @property {SingleValueChart.onSeriesClick} [onSeriesClick] - The callback function which will be fired if a series point is clicked.
 * @property {SingleValueChart.onChartLoaded} [onChartLoaded=null] - A callback can be register which will be invoked after chart is data is loaded and it is going to render. 
 * @property {boolean}  [xAxisCustomInfoLabel=false] - If true, the default subgroup number label for xAxis will be overridden by the property of customInfo object, where xAxisLabel is true. For more information  please refer {@link SingleValueChart.customInfo}
 * @property {SingleValueChart.yAxisUnit} [yAxisUnit] - The unit of y Axis object.
 * @property {boolean} [splitTooltip = true] - By default, the tool-tip will be shown for each series individually. By setting this to false, each series values will be shown by selecting an individual point. 
 * @property {SingleValueChart.legendSettings} legendSettings - Settings for position, layout and visibility of the chart legend
 * @property {SingleValueChart.tooltip} [series] - series & tooltip related settings. Series order should be 1 and less than lgend items of series.  
 * @property {SingleValueChart.yAxisScale}  [yAxisScale] -determine the min/max value of the yAxis if the user provides values for min and max then minScaling and maxScaling does not affect the scaling either the user specifies numeric values for min and max or specifies the scaling behavior according to {@link scaling}
 * @property {boolean}  [animation=false] -Enable or disable the initial animation
 * @property {SingleValueChart.titleSettings} [options.titleSettings]  - Provides options to position & rotate the chart title
 * @property {SingleValueChart.navigator} [options.navigator]  - The navigator object is to define start and end of navigator range as well as the number of selected values
 * @property {SingleValueChart.fontSize} [fontSize]-Option to set Fontsize of label & title.
 * @property {number} [xAxisLabelsRotation] Rotation of the labels in degrees. When undefined, the engine will try to hide/autorotate lables to avoid overlapping.
 * @property {SingleValueChart.axis} [SingleValueChart.xAxis] - Settings for font, fontsize and color of xAxis labels 
 * @property {SingleValueChart.axis} [SingleValueChart.yAxis] - Settings for font, fontsize and color of yAxis labels 
 */

/**
 * @description the behavior of series, whether to hide from legend and / or initial visible. 
 *      Default for all additional lines is invisible and not shown in legend, all others are shown in legend and are visible.
 * @typedef {object} seriesVisibilityObject
 * @memberof SingleValueChart
 * @property {string} [seriesId] - the seriesId to manipulate {@link SingleValueChart.seriesIds}
 * @property {boolean} [showInChart] - initial show this series or not
 * @property {boolean} [showInLegend] - display this series in the legend
 * @example
 *  $('#svc').singleValueChart(
 *      {
 *          locale: "en",
 *          data: {
 *                  specifications : {
 *                                      subgroupSize : 2,
 *                                      controlChartType : 'xb_s',
 *                                  }
 *                  measurements : [], // Array of objects in defined format
 *                  subgroups: [],  // Array of objects in defined format
 *              },
 *           decimalPlaces: 2,
 *           xAxisCustomInfoLabel: true,
           
 *           "AdditionalLines": {
 *             "firstChoiceUpperLimit": 20.01,
 *             "firstChoiceLowerLimit": 19.99,
 *             "secondChoiceUpperLimit": 20.02,
 *             "secondChoiceLowerLimit": 19.98,
 *             "thirdChoiceUpperLimit": 20.04,
 *             "thirdChoiceLowerLimit": 19.96
 *           },
 *           xAxis: {
 *             labels: {
 *              fontSize: '10pt',
 *              font: 'Arial',
 *              color: 'red'
 *             }
 *           },
 *           yAxis: {
 *             labels: {
 *               fontSize: '12pt',
 *               font: 'Arial',
 *               color: 'blue'
 *             }
 *           },           
 *           hiddenSeries: [
 *              {
 *                  "seriesId": "firstChoiceUpperLimit",
 *                  "showInChart": false,
 *                  "showInLegend": true
 *               },
 *              {
 *                  "seriesId": "secondChoiceUpperLimit",
 *                  "showInChart": false,
 *                  "showInLegend": true
 *               },
 *               {
 *                  "seriesId": "thirdChoiceUpperLimit",
 *                  "showInChart": false,
 *                  "showInLegend": true
 *               }
 *           ]
 *      });
 */

/**
 * @typedef {object} axis
 * @memberof SingleValueChart
 * @description Option to set font, fontsize and color of axis labels.
 * @property {string} [fontSize]- Fontsize for labels.
 * @property {string} [font]- Font for labels.
 * @property {string} [color]- Color for labels.
 * @example
 *   xAxis: {
 *      labels: {
 *        fontSize: '10pt',
 *        font: 'Arial',
 *        color: 'red'
 *      }
 *   },
 *   yAxis: {
 *      labels: {
 *        fontSize: '10pt',
 *        font: 'Arial',
 *        color: 'red'
 *      }
 *   }
 */

/**
 * @description The titleSettings object is to determine the position and rotation of the chart title
 * @typedef {object} titleSettings
 * @memberof SingleValueChart
 * @property {string} [align = undefined] - The horizontal alignment of the title. Can be one of "left", "center" and "right".
 * @property {number} [rotate = undefined] - The rotation of the text in degrees. 0 is horizontal, 270 is vertical reading from bottom to top.
 * @property {number} [x = undefind] - The x position of the title relative to the alignment
 * @property {number} [y = undefind] - The y position of the title relative to the alignment
 * @example
 * titleSettings:{
 *  align: 'left',
 *  rotate: '90',
 *  x: '35',
 *  y:'207'
 * },
 */

/**
 * @description The navigator object is to define start and end of navigator range as well as the number of selected values
 * @typedef {object} navigator
 * @memberof SingleValueChart
 * @property {number} [start = undefined] - first value to be selected in the navigator when defined alone then the last n values would be selected.
 * @property {number} [end = undefined] - last value to be selected in the navigator when defined alone then the first n values would be selected
 * @property {number} [selected = 25] - the number of selected value in the navigator, if start and end are set this value has no effect.
 * @example
 *navigator: {
 *               start: 0,
 *              end: 5,
 *               selected: 5
 *           },
 */

/**
 * @description the values to set the legend to the preferred position or to hide it totally
 * @typedef {object} legendSettings
 * @memberof SingleValueChart
 * @property {boolean} [enabled = true] - show or hide the legend.
 * @property {string} [align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */
/**
 * @description The yAxisScale object is to determine the min/max value of the yAxis if the user provide values for Min and Max then MinScaling and MaxScaling does not affect the scaling either the user specifies numeric values for min and mix or specifies the scaling behavior according to {@link scaling}
 * @typedef {object} yAxisScale
 * @memberof SingleValueChart
 * @property {number} [min = undefined] - The minimum value of the axis. If null or undefined the minimum value is calculated based on {@link scaling}.
 * @property {number} [max = undefined] - The maximum value of the axis. If null or undefined the maximum value is calculated based on {@link scaling}.
 * @property {scaling} [minScaling = scaling.default] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {scaling} [maxScaling = scaling.default] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 */

/**
	* @description An array of objects with following properties can be injected against each measurements.
	* @typedef {object} tooltip
	* @memberof SingleValueChart
	* @property {boolean} [showInTooltip=true] - To show or hide a series in tooltip
	* @property {string} seriesId - The id of the series
	* @property {number} order - The order in which a series should be displayed in tooltip. Starting from 1. 
	* @example
	*  $('#svc').singleValueChart(
	*      {
	*          locale: "en",
	*          data: {
	*               specifications : 
	*                   {
	*                       subgroupSize : 2,
	*                       controlChartType : 'xb_s',
	*                   }
	*               measurements : [], // Array of objects in defined format
	*               subgroups: [],  // Array of objects in defined format
	*              },
	*           decimalPlaces: 2,
	*           xAxisCustomInfoLabel: true,
	*           series: {
	*               tooltip: [
	*                   {
	*                       seriesId: 'ltl',
	*                       order: 1,
	*                       showInTooltip: true
	*                   },
	*                   {
	*                       seriesId: 'lcl',
	*                       order: 2,
	*                       showInTooltip: true
	*                   },
	*                   {
	*                       seriesId: 'lwl',
	*                       order: 3,
	*                       showInTooltip: true
	*                   },
	*                   {
	*                       seriesId: 'xbp',
	*                       order: 4,
	*                       showInTooltip: false
	*                   },
	*                   {
	*                       seriesId: 'uwl',
	*                       order: 5,
	*                       showInTooltip: true
	*                   },
	*                   {
	*                       seriesId: 'ucl',
	*                       order: 6,
	*                       showInTooltip: true
	*                   },
	*                   {
	*                       seriesId: 'utl',
	*                       order: 7,
	*                       showInTooltip: true
	*                   },
	*                   {
	*                       seriesId: 'measurements__1',
	*                       order: 9,
	*                       showInTooltip: true
	*                   },
	*                   {
	*                       seriesId: 'nominalValue',
	*                       order: 10,
	*                       showInTooltip: true
	*                   }
	*               ]
	*           }
	*      });
*/

/**
 * @description An array of objects with following properties can be injected against each measurements.
 * @typedef {object} customInfo
 * @memberof SingleValueChart
 * @property {string} label - The label to be display in tool tip .
 * @property {string} value - The value to be used to show in tool tip  and for xAxis labels.
 * @property {boolean} showInTooltip - if true, the label and value will also be shown in tool tip for a particular subgroup as 'label: value'
 * @property {boolean} xAxisLabel - If true then value of will be shown on xAxis labels instead of default labels. e.g. subgroup number
 * @property {boolean} isRemark - If this flag is set then the value will be shown when in <b>measurement</b> (input object to charts) contains <b>statuses</b> property with category </b>Remark</b>
 * @example
 * <caption>A customInfo can be injected like this, if a data is defined in the given {@link SingleValueChart.data}</caption>

	data.measurements[0].customInfo = [
		{
			label: 'Charge Number',
			value: 'CH001',
			showInTooltip: true,
			xAxisLabel: false
		},
		{
			label: 'Date',
			value: '05-01-2020',
			showInTooltip: true,
			xAxisLabel: true,
			isRemark: true
		}
	];
  // Note: If against a subgroup more than one objects have 'xAxisLabel' true then the value by default
  // of the first object will be taken. e.g. If in the given example both objects have xAxisLabel true
  // then in chart the value of first object on xAxis label (for subgroup 0) will be displayed.
  // Which is in this case 'CH001'
 */

/**
 * @typedef {object} yAxisUnit
 * @memberof SingleValueChart
 * @description To specify the yAxis unit related properities.
 * @property {string} [text=""] - To specify the series colors.
 * @property {number} [x=60] - x position of the text.
 * @property {number} [y=null] - y position of the text. 
 * @property {number|string} [fontSize=null] - The font size of the text. If as number will be given then unit will be in pixel. As string in points can be size given e.g. '12pt'
 * @example
 * $('#svc').singleValueChart(
 *      {
 *          locale: "en",
 *          data: {
 *                  specifications: {
 *                              subgroupSize : 2,
 *                              controlChartType : 'xb_s'
 *                              },
 *                  measurements : [], // Array of objects in defined format
 *                  subgroups: [],  // Array of objects in defined format
 *           },
 *           decimalPlaces: 2,
 *           xAxisCustomInfoLabel: true,
 *           "AdditionalLines": {
 *             "secondChoiceUpperLimit": 20.02,
 *             "secondChoiceLowerLimit": 19.98,
 *             "thirdChoiceUpperLimit": 20.04,
 *             "thirdChoiceLowerLimit": 19.96
 *           },
 *           hiddenSeries: [
 *           {
 *              seriesId: thirdChoiceUpperLimit,
 *              showInLegend: true,
 *              showInChart: false
 *           },
 *           {
 *              seriesId: secondChoiceUpperLimit,
 *              showInLegend: true,
 *              showInChart: false
 *           }
 *           ],
 *           onSeriesClick: function (data) {
 *                      console.log(data);
 *           },
 *           onLegendClick : function (data) {
 *                      console.log(data);
 *           },
 *           yAxisUnit: {
 *                  text: 'DW05Unit01',
 *                  x: 100,
 *                  y: -20,
 *                  fontSize: '20pt'
 *                  }
 *        });
 */
/**
 * @typedef {object} fontSize
 * @memberof SingleValueChart
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
			title: 20,
			labels: 15
	}
 */

$.fn.singleValueChart = function (options) {
	var settings = {},
		tooltipRow = '<tr series_id="{2}"><td style="text-align: left; padding: 0;">{0}</td> <td style="text-align: right; padding: 0;">{1}</td></tr>',
		tooltipHr = '<tr><td colspan="2;" style = "padding: 3px 4px 3px 4px;"><hr style="margin-top: 0; padding: 0; margin-bottom: 0;"/></td></tr>',
		tooltipTableStart = '<table style="border-spacing: 0px;"><tbody>',
		tooltipTableEnd = '</tbody></table>';

	if (options && typeof options === 'object') {   // if not a method call
		var _options = setDefaults(options);
		
		settings = $.extend({
			chartType: chartTypes.svc,
			locale: 'en',
			data: {}, // JSON Array with subgroup
			chartTitle: getLocalizedText('svc_chart', _options.locale),
			decimalPlaces: 4,
			xAxisTitle: undefined,
			yAxisTitle: undefined,
			height: undefined,
			width: undefined,
			fontSize: {
				title: undefined,
				labels: undefined
			},
			onSeriesClick: undefined,
			yAxisScale: {
				min: undefined,
				max: undefined,
				minScaling: scaling.default,
				maxScaling: scaling.default
			},			
			navigator: {
				start: undefined,
				end: undefined,
				selected: 25
			},
			onLegendClick: undefined,
			onChartLoaded: undefined,
			yAxisUnit: getDefaultyAxisUnitObj(),
			hiddenSeries: [], //TODO: Clear with TM, wether is this right that additional lines can be enabled through hidden series object and not based on data?
			splitTooltip: true,
			siemensTooltip: {},
			legendSettings: {
				enabled: undefined,
				align: undefined,
				verticalAlign: undefined,
				layout: undefined,
				x: undefined,
				y: undefined
			},
			titleSettings: undefined,
			animation: false,
			xAxis: {
				labels: {
					fontSize: '11px',
					font: 'Arial',
					color: 'black'
				}
			},
			yAxis: {
				labels: {
					fontSize: '11px',
					font: 'Arial',
					color: 'black'
				}
			},
			//additionalLines: { //TODO: What is this. Nowhere used but data object is used for additional lines.
			//    firstChoiceUpperLimit: undefined,
			//    firstChoiceLowerLimit: undefined,
			//    secondChoiceUpperLimit: undefined,
			//    secondChoiceLowerLimit: undefined,
			//    thirdChoiceUpperLimit: undefined,
			//    thirdChoiceLowerLimit: undefined,
			//    centerOfTolerances: undefined,
			//    averageOfAllValues: undefined,
			//    oneThirdOfUpperControlLimit: undefined,
			//    twoThirdsOfUpperControlLimit: undefined
			//},
			series: getDefaultSeriesObj()//TODO: HiddenSeries should be removed and series object should be extend to show or hide series in legend and in chart
		}, _options, { chartType: chartTypes.svc });
	} else if (typeof options === 'string') { // Method call
		settings = $(this).data('settings') || {};  // restore the settings object from prev settings
	}

	var $this = this,
		_args = arguments,
		chart = {
			init: function () {
				var containerId = $this.attr('id'),
					input = {
						series: [], ucl: [], lcl: [],
						uwl: [], lwl: [], xbp: [],
						utl: [], ltl: [], data: settings.data,
						sortNumbers: [],
						customInfo: [], sequenceIds: [],
						flagSeries: [], nominalValue: [],
						firstChoiceUpperLimit: [], firstChoiceLowerLimit: [], secondChoiceUpperLimit: [], secondChoiceLowerLimit: [], thirdChoiceUpperLimit: [], thirdChoiceLowerLimit: [],
						centerOfTolerances: [], averageOfAllValues: [], oneThirdOfUpperControlLimit: [], twoThirdsOfUpperControlLimit: [], oneThirdOfLowerControlLimit: [], twoThirdsOfLowerControlLimit: [],
						additionalMeasurements: [], upperMiddlethird: [], lowerMiddlethird: []
					};

				if (settings.data && settings.data.subgroups && settings.data.measurements && settings.data.specifications) {
					prepareData(input);

					settings.inputs = input;    // for tooltip etc.

					var _chart = Highcharts.stockChart(containerId, prepareChartOptions(input));
					//  chart.xAxis[0].setExtremes(75, 100);	// Initial select last 25 samples
					$this.data('chartApi', _chart);
					// $this.data('inputData', input); // to get the series data on method call
					$this.data('settings', settings);

				} else
					throw 'Invalid data format exception: Expected data is not in defined format. Please consult developer documentation.';
			},

			/**
			 * @memberof SingleValueChart
			 * @function addMeasurement
			 * @description To add a new point in measurment series.
			 * @param {object|number} newMeasurement - If The value of the point that need to added. Only measurement series will be updated. <br> The corresponding limit will be increase based on their last values.
			 * <br> If the limits need to be update manually then an object should be passed as mentioned in the example below
			 *
			 * @example
			 * //To add just a new point in single value chart
			 * $('#SVC').singleValueChart('addMeasurement', 20.09);
			 * // To just add a new measurement value. The limits e.g. control limit etc. will be taken from previous measurement.
			 *
			 * // To add new measurement values with limits
			 * $('#SVC').singleValueChart('addMeasurement', {
			 *     measurement: 20.14,  // new measurment value
			 *     lcl: 10.03,  // lower control limit
			 *     ucl: 20.09,  // upper control limit
			 *     lwl: 10.02,  // lower warn limit
			 *     uwl: 20.05,  // upper warn limit
			 *     ltl: 19.65,  // lower tolerance limit
			 *     utl: 19.95,  // upper tolerance limit
			 *     xbp: 20.001, // process mean value
			 *     nominalValue: 20.011, // nominalValue (if different from subgroup)
			 *     averageOfAllValues: 20.125,  //average of all mesured valus
			 *     additionalMeasurement: 20.77,  //for additional mesurements, if needed
			 *     customInfo: [
			 *                   {
			 *                       label: 'custon Info',
			 *                       value: 'added by function',
			 *                       showInTooltip: true,
			 *                       xAxisLabel: false
			 *                   },
			 *                   {
			 *                       label: '2. custo Info',
			 *                       value: 'info for tooltip',
			 *                       showInTooltip: true,
			 *                       xAxisLabel: true,
			 *                       isRemark: true
			 *                   }
			 *               ]
			 *   });
			 *
			 */

			addMeasurement: function (newPoint) {
				var inputObject = {},
					inputNumber = roundFloat(newPoint, null, true);
				if (inputNumber != null /*typeof newPoint === 'number'*/) {
					inputObject.measurement = inputNumber; //  newPoint;
				} else if (newPoint && typeof newPoint === 'object') {
					inputObject = newPoint;
				} else { throw 'The passed value is not a valid number '; }

				addSeriesPoints(inputObject);
			},

			/**
			* @description - Tooltip can be switched between combined and individual tooltip
			* @memberof SingleValueChart
			* @function splitTooltip
			* @param {boolean} state - True will show the tooltip for each series individually and false will combine the tooltip.             
			* @example
			* $('#chart').chart('splitTooltip', true);
			*
			*/
			splitTooltip: function (state) {
				var chartApi = $this.data('chartApi');
				if (state === true) {
					settings.siemensTooltip.split = false;
					chartApi.update({ tooltip: settings.siemensTooltip });
					settings.splitTooltip = true;
				} else if (state === false) {
					settings.siemensTooltip.split = true;
					chartApi.update({ tooltip: settings.siemensTooltip });
					settings.splitTooltip = false;
				}
			},

			/**
			* @description - measurements may be filtered by the values of a specific customInfo. There can be a Choice of values, separated by colon.
			* @memberof SingleValueChart
			* @function filterSVC
			* @param {array} filters - an Array of objects [ { label: 'Charge Number', value: 'CH00479;CH00480;CH00481;CH00482;CH00483;CH00484' }]
			* @example
			* $('#SVC').singleValueChart('filterSVC', [ { label: 'Charge Number', value: 'CH00479;CH00480;CH00481;CH00482;CH00483;CH00484' }]
			*                                         [ { label: 'Color', value: 'red;blue' }] );
			*This filter looks in every measurement for 
			*       custom info called 'Charge Number' with one of the values in 'CH00479;CH00480;CH00481;CH00482;CH00483;CH00484'
			*       OR
			*       custom info called 'Color' with one of the values in 'red;blue'
			*       
			* Only the measurement with matching filters are displayed      
			* 
			*/
			filterSVC: function (filters) {
				var input = $this.data('settings').inputs;
				var measurementssave = $this.data('settings').inputs.data.measurements;
				var filteredMesurements = [];
				var addMeasurement = false;
				if (!filters || filters.length == 0 || filters[0].label == "" || filters[0].value == "") {
					filteredMesurements = measurementssave;
				}
				else {
					for (var i = 0; i < measurementssave.length; i++) {
						addMeasurement = false;
						if (measurementssave[i].customInfo && measurementssave[i].customInfo.length > 0) {
							for (var j = 0; j < measurementssave[i].customInfo.length; j++) {
								if (addMeasurement) break;
								for (var k = 0; k < filters.length; k++) {
									if (addMeasurement) break;

									if (measurementssave[i].customInfo[j].label == filters[k].label && measurementssave[i].customInfo[j].value == filters[k].value) {
										addMeasurement = true;
									}
								}
							}

						}
						if (addMeasurement) filteredMesurements.push(measurementssave[i]);
					};
				}

				//input.series.length = 0;
				//input.sortNumbers.length = 0;
				//input.sequenceIds.length = 0;
				//input.ucl.length = 0;
				//input.lcl.length = 0;
				//input.uwl.length = 0;
				//input.lwl.length = 0;
				//input.xbp.length = 0;
				//input.utl.length = 0;
				//input.ltl.length = 0;
				//input.customInfo.length = 0;
				//input.flagSeries.length = 0;
				//input.nominalValue.length = 0;



				settings.data.measurements = filteredMesurements;
				var containerId = $this.attr('id'),
					input = {
						series: [], ucl: [], lcl: [],
						uwl: [], lwl: [], xbp: [],
						utl: [], ltl: [], data: settings.data, // may be this data can be taken out
						sortNumbers: [], /*for xAxis labels, as it can be custom based on cutomInfo*/
						customInfo: [], sequenceIds: [],//for tooltip
						flagSeries: [], nominalValue: [],
						firstChoiceUpperLimit: [], firstChoiceLowerLimit: [], secondChoiceUpperLimit: [], secondChoiceLowerLimit: [], thirdChoiceUpperLimit: [], thirdChoiceLowerLimit: [],
						centerOfTolerances: [], averageOfAllValues: [], oneThirdOfUpperControlLimit: [], twoThirdsOfLowerControlLimit: []
					};

				if (settings.data && settings.data.subgroups && settings.data.measurements && settings.data.specifications) {
					// siemensTooltip.formatter = displayTooltip; 
					prepareData(input);

					settings.inputs = input;    // for tooltip etc.

					var _chart = Highcharts.stockChart(containerId, prepareChartOptions(input));
					//  chart.xAxis[0].setExtremes(75, 100);	// Initial select last 25 samples
					$this.data('chartApi', _chart);
					// $this.data('inputData', input); // to get the series data on method call
					$this.data('settings', settings);

				} else
					throw 'Invalid data format exception: Expected data is not in defined format. Please consult developer documentation.';

				$this.data('chartApi').redraw();
				$this.data('settings').inputs.data.measurements = measurementssave;
			},

			/**
			 * @description This method update the value of a measurement series point. The point can be updated based on provided sequence Id.
			 * @memberof SingleValueChart
			 * @function updatePoint
			 * @param {number} sequenceId - The sequence Id of the point that need to be updated.
			 * @param {number} newValue - The new Value of the Point that need to be updated.

			 * @example
			 * $('#SVC').singleValueChart('updatePoint', 500, 20.09);
			 * // This will update the 500th measurment value with 20.09, if 500th measurement is available.
			 * // Otherwise no update will be done. On console the corresponding error can be then seen.

			**/
			updatePoint: function (sequenceId, newValue) {
				var __chart = $this.data('chartApi');
				if (__chart) {
					if (__chart.get(seriesIds.singleValueChart.measurement)) { // only measurement series allowed to update
						var data = __chart.get(seriesIds.singleValueChart.measurement).data[sequenceId];

						if (data)
							data.update(newValue);
						else
							console.error('The sequence Id ' + sequenceId + ' is not found');
					}
				}
			},

			/**
		 * @description This method add/update the remark string for statuses <b>Remark</b> in <b>measurement</b> object. Usually <b>Remark</b> object is shown as green diamond shape. 
		 * @memberof SingleValueChart
		 * @function addRemarks
		 * @param {number} sequenceId - The sequence Id of the point where a remark should be added.
		 * @param {string} remarks - The new remark text.

		 * @example
		 * $('#SVC').singleValueChart('addRemarks', 397, 'new remark')
		 * // If there is already a remark through customInfo passed, it will be updated. In other case new remark will be added for corresponding sequence id.

		**/
			addRemarks: function (sequenceId, remarks) {
				var customInfo = settings.inputs.customInfo[sequenceId];
				if (customInfo) {
					if (customInfo.length > 0) {
						var remarksObj = customInfo.filter(function (k) { return k.isRemark === true; });
						if (remarksObj.length > 0) {
							remarksObj[0].value = remarks;
						}
						else {
							customInfo[sequenceId][0].value = remarks;
							customInfo[sequenceId][0].isRemark = true;
						}
					}
					else {
						settings.inputs.customInfo[sequenceId].push(
							{
								value: remarks,
								isRemark: true
							}
						);
					}
				}
			},

			/**
			* @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
			* @memberof SingleValueChart
			* @function createBase64Image
			* @param {object} options - An object with specified properties
			* @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
			* @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
			* @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
			* @param  {Function} options.success - Callback when chart is rendered.
			* @param {Function} [options.error] - Callback to get error information.
			* @requires exporting.js
			* @requires offline-exporting.js
			* @requires canvg.js - For Internet explorer only
			* @example
			* $('#singleValueChart').singleValueChart('createBase64Image',
			*                                       {
			*                                           containerId : 'sImageBase64',
			*                                           width : 1800,
			*                                           height: 600,
			*                                           success: function(){
			*                                                               console.log(document.getElementById('sImageBase64').value);
			*                                           },
			*                                           error: function(e) {
			*                                                               console.log(e);
			*                                           }
			*                                       });
			*/
			createBase64Image: function (options) {
				var chartApi = $this.data('chartApi');
				createBase64Image(chartApi, options);
			}
		};
	return this.each(function () {
		if (chart[options]) {
			return chart[options]
				(_args[1], _args[2], _args[3], _args[4]);
		} else if (typeof options === 'object' || !options) {
			chart.init();
		}
	});

	function prepareData(input) {
		var hasSubgroupAdditionalLines = false;
		if (input.data && input.data.subgroups && input.data.subgroups.filter) {
			hasSubgroupAdditionalLines = input.data.subgroups.filter(function (k) { return !isNullOrUndefined(k.additionalLines); }).length > 0;
		};
		if (input.data && input.data.additionalMeasurements) {
			for (var i = 0; i < input.data.additionalMeasurements.length; i++) {
				input.additionalMeasurements.push(roundFloat(input.data.additionalMeasurements[i], null, true));
			}
		}

		$.each(input.data.measurements, function (i, p) {
			var data = input.data,
				// subgroup number is now correctly provided from API
				// subgrpNum = isSubequalMeasure(data.specifications.controlChartType) ? i : p.subgroupNumber,
				statuses = p.statuses ? getVoilations(p.statuses) : [],
				val = roundFloat(p.measuredValue),
				subgroup = findSubgroupByNumber(data.subgroups, p.subgroupNumber) || {},
				customInfo = p.customInfo || [], // May be not each subgroup contain customInfo, therefore empty object should be used for corresponding subgroup
				toolChanged_status = getMeasurmentStatuses(p, statusesCategory.toolchanged) || [],
				remark_status = getMeasurmentStatuses(p, statusesCategory.remark) || [],
				attachment_status = getMeasurmentStatuses(p, statusesCategory.attachment) || [],
				ucl = roundFloat(subgroup.upperControlLimit1Abs, null, true),
				lcl = roundFloat(subgroup.lowerControlLimit1Abs, null, true),
				uwl = roundFloat(subgroup.upperWarnLimit1Abs, null, true),
				lwl = roundFloat(subgroup.lowerWarnLimit1Abs, null, true),
				xbp = roundFloat(subgroup.calculatedProcessMeanValue1, null, true),
				utl = roundFloat(subgroup.upperToleranceLimitAbs, null, true),
				ltl = roundFloat(subgroup.lowerToleranceLimitAbs, null, true),
				nominalValue = roundFloat(subgroup.nominalValue, null, true);

			if (p.upperToleranceLimitAbs != null) {
				utl = roundFloat(p.upperToleranceLimitAbs, null, true)
			} 
			if (p.lowerToleranceLimitAbs != null) {
				ltl = roundFloat(p.lowerToleranceLimitAbs, null, true)
			}
			if (p.nominalValue != null) {
				nominalValue = roundFloat(p.nominalValue, null, true)
			}

			addAdditionalLines(input, subgroup, hasSubgroupAdditionalLines);

			if (statuses.indexOf('processviolation_1') > -1 || statuses.indexOf('processviolation_2') > -1)
				input.series.push({
					marker: {
						fillColor: chartSeriesColors.SingleValueChart.Marker,
						lineWidth: 3,
						lineColor: chartSeriesColors.SingleValueChart.Marker,
						symbol: 'triangle'
					},
					name: 'violation' + i,
					y: val,
					vType: statuses
				});
			else if (statuses.indexOf('outlier') > -1) {
				input.series.push({
					marker: {
						fillColor: chartSeriesColors.SingleValueChart.YellowMarker,
						lineWidth: 3,
						lineColor: chartSeriesColors.SingleValueChart.YellowMarker,
						symbol: 'triangle'
					},
					name: 'outlier' + i,
					y: val,
					vType: statuses
				});
			} else if (statuses.indexOf("eliminated") > -1) {
				input.series.push({
					marker: {
						fillColor: chartSeriesColors.SingleValueChart.Marker,
						lineWidth: 3,
						lineColor: chartSeriesColors.SingleValueChart.Marker,
						symbol: 'cross'
					},
					name: 'eliminated',
					y: val,
					vType: statuses
				})
			} else if (statuses.indexOf("eliminatedbycalculation") > -1) {
				input.series.push({
					marker: {
						fillColor: chartSeriesColors.SingleValueChart.Marker,
						lineWidth: 3,
						lineColor: chartSeriesColors.SingleValueChart.Marker,
						symbol: 'cross'
					},
					name: 'eliminatedbycalculation',
					y: val,
					vType: statuses
				});
			}
			else
				input.series.push(val);

			if (settings.xAxisCustomInfoLabel === true) {
				if (customInfo) {
					input.sortNumbers.push(getCustomxAxisLabel(customInfo));
				} else { input.sortNumbers.push(null); }
			} else
				input.sortNumbers.push(roundFloat(p.sequenceID));

			input.sequenceIds.push(roundFloat(p.sequenceID)); //tooltip

			input.ucl.push(ucl);
			input.lcl.push(lcl);
			input.uwl.push(uwl);
			input.lwl.push(lwl);
			input.xbp.push(xbp);
			input.utl.push(utl);
			input.ltl.push(ltl);
			input.nominalValue.push(nominalValue);
			p.violations = subgroup.statuses;

			if (p.subgroupNumber == null) {
				input.ucl[i] = input.ucl[i - 1];
				input.lcl[i] = input.lcl[i - 1];
				input.uwl[i] = input.uwl[i - 1];
				input.lwl[i] = input.lwl[i - 1];
				input.xbp[i] = input.xbp[i - 1];
				input.utl[i] = input.utl[i - 1];
				input.ltl[i] = input.ltl[i - 1];
				input.nominalValue[i] = input.nominalValue[i - 1];
			}

			if (toolChanged_status.length > 0) {

				input.flagSeries.push({
					y: val,
					x: i,
					title: svgRepository.ToolChanged,
					text: getLocalizedText('ToolChanged', settings.locale),
				});
			}
			if (remark_status.length > 0) {
				input.flagSeries.push({
					y: val,
					x: i,
					title: svgRepository.Remark,
					text: getCustomRemarks(settings, i)
				});
			}
			if (attachment_status.length > 0) {
				input.flagSeries.push({
					y: val,
					x: i,
					title: svgRepository.Attachment,
					text: getLocalizedText('Attachment', settings.locale),

				});
			}

			input.customInfo.push(customInfo);
		});

		delete input.data.subgroups.currentSelectedSubgroupSelected; // remove extra property which is set for tracking
	}

	function prepareSeries(input) {
		
		var ser = [
			{
				id: seriesIds.singleValueChart.measurement,
				name: getLocalizedText('measurement', settings.locale),
				animation: settings.animation,
				data: input.series,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.Measurement,
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: true,
					symbol: 'circle',
					radius: 4
				},
				legendIndex: 1,
				showInLegend: true,
				visible: true,
				events: {
					click: onSeriesClick
				},
				zoneAxis: 'x',
				zones: prepareColoredZone()
			},
			{
				id: seriesIds.singleValueChart.additionalMeasurement,
				name: getLocalizedText('AdditionalMeasurement', settings.locale),
				animation: settings.animation,
				data: input.additionalMeasurements,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.additionalMeasurement,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.additionalMeasurement, input.additionalMeasurements, settings.hiddenSeries, true).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.additionalMeasurement, input.additionalMeasurements, settings.hiddenSeries, true).showInChart,
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: true,
					symbol: 'circle',
					radius: 4
				},
				legendIndex: 2,
				events: {
					click: onSeriesClick
				},
				zoneAxis: 'x'
			},
			{
				id: seriesIds.singleValueChart.utl,
				name: getLocalizedText('UTL_1', settings.locale), //  Highcharts.uiLocale[settings.locale].UTL_1,
				animation: settings.animation,
				data: input.utl,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.ToleranceLimit,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 3,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.utl, input.utl, settings.hiddenSeries, true).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.utl, input.utl, settings.hiddenSeries, true).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.ucl,
				name: getLocalizedText('UCL_1', settings.locale), //  Highcharts.uiLocale[settings.locale].UCL_1,
				animation: settings.animation,
				data: input.ucl,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.ControlLimit,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 4,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.ucl, input.ucl, settings.hiddenSeries, true).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.ucl, input.ucl, settings.hiddenSeries, true).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.uwl,
				name: getLocalizedText('UWL_1', settings.locale),
				animation: settings.animation,
				data: input.uwl,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.WarningLimit,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 5,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.uwl, input.uwl, settings.hiddenSeries, true).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.uwl, input.uwl, settings.hiddenSeries, true).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.xbp,
				name: getLocalizedText('xbp_1', settings.locale), //  Highcharts.uiLocale[settings.locale].xbp_1,
				animation: settings.animation,
				data: input.xbp,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.xbp,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 6,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.xbp, input.xbp, settings.hiddenSeries, true).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.xbp, input.xbp, settings.hiddenSeries, true).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.lwl,
				name: getLocalizedText('LWL_1', settings.locale), //  Highcharts.uiLocale[settings.locale].LWL_1,
				animation: settings.animation,
				data: input.lwl,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.WarningLimit,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 7,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.lwl, input.lwl, settings.hiddenSeries, true).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.lwl, input.lwl, settings.hiddenSeries, true).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.lcl,
				name: getLocalizedText('LCL_1', settings.locale), //  Highcharts.uiLocale[settings.locale].LCL_1,
				animation: settings.animation,
				data: input.lcl,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.ControlLimit,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 8,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.lcl, input.lcl, settings.hiddenSeries, true).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.lcl, input.lcl, settings.hiddenSeries, true).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.ltl,
				name: getLocalizedText('LTL_1', settings.locale), //  Highcharts.uiLocale[settings.locale].LTL_1,
				animation: settings.animation,
				data: input.ltl,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.ToleranceLimit,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 9,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.ltl, input.ltl, settings.hiddenSeries, true).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.ltl, input.ltl, settings.hiddenSeries, true).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.firstChoiceUpperLimit,
				name: getLocalizedText('firstChoiceUpperLimit', settings.locale),
				animation: settings.animation,
				data: input.firstChoiceUpperLimit,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.FirstChoice,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 10,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.firstChoiceUpperLimit, input.firstChoiceUpperLimit, settings.hiddenSeries, false).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.firstChoiceUpperLimit, input.firstChoiceUpperLimit, settings.hiddenSeries, false).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.flagSeries,
				name: seriesIds.singleValueChart.flagSeries,
				animation: settings.animation,
				type: 'flags',
				data: input.flagSeries,
				onSeries: seriesIds.singleValueChart.measurement,
				showInLegend: false,
				useHTML: true,
				dataLabels: {
					useHTML: true,
				},
				lineWidth: 1,
				lineColor: '#005F87',
				stackDistance: 10
			},
			{
				id: seriesIds.singleValueChart.firstChoiceLowerLimit,
				linkedTo: seriesIds.singleValueChart.firstChoiceUpperLimit,
				//name: getLocalizedText('firstChoiceLowerLimit', settings.locale),
				animation: settings.animation,
				data: input.firstChoiceLowerLimit,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.FirstChoice,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 11,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.secondChoiceUpperLimit,
				name: getLocalizedText('secondChoiceUpperLimit', settings.locale),
				animation: settings.animation,
				data: input.secondChoiceUpperLimit,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.SecondChoice,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 12,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.secondChoiceUpperLimit, input.secondChoiceUpperLimit, settings.hiddenSeries, false).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.secondChoiceUpperLimit, input.secondChoiceUpperLimit, settings.hiddenSeries, false).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.secondChoiceLowerLimit,
				linkedTo: seriesIds.singleValueChart.secondChoiceUpperLimit,
				//name: getLocalizedText('secondChoiceLowerLimit', settings.locale), 
				animation: settings.animation,
				data: input.secondChoiceLowerLimit,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.SecondChoice,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 13,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.thirdChoiceUpperLimit,
				name: getLocalizedText('thirdChoiceUpperLimit', settings.locale),
				animation: settings.animation,
				data: input.thirdChoiceUpperLimit,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.ThirdChoice,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 14,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.thirdChoiceUpperLimit, input.thirdChoiceUpperLimit, settings.hiddenSeries, false).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.thirdChoiceUpperLimit, input.thirdChoiceUpperLimit, settings.hiddenSeries, false).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.thirdChoiceLowerLimit,
				linkedTo: seriesIds.singleValueChart.thirdChoiceUpperLimit,
				//name: getLocalizedText('thirdChoiceLowerLimit', settings.locale), 
				animation: settings.animation,
				data: input.thirdChoiceLowerLimit,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.ThirdChoice,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 15,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.controlChart.centerOfTolerances,
				name: getLocalizedText('centerOfTolerances', settings.locale),
				animation: settings.animation,
				data: input.centerOfTolerances,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.CenterOfTolerances,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 16,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.centerOfTolerances, input.centerOfTolerances, settings.hiddenSeries, false).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.centerOfTolerances, input.centerOfTolerances, settings.hiddenSeries, false).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.averageOfAllValues,
				name: getLocalizedText('averageOfAllValues', settings.locale),
				animation: settings.animation,
				data: input.averageOfAllValues,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.averageOfAllValues,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 17,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.averageOfAllValues, input.averageOfAllValues, settings.hiddenSeries, false).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.averageOfAllValues, input.averageOfAllValues, settings.hiddenSeries, false).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.oneThirdOfUpperControlLimit,
				name: getLocalizedText('oneThirdOfControlLimits', settings.locale),
				animation: settings.animation,
				data: input.oneThirdOfUpperControlLimit,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.oneThirdOfControlLimits,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 18,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.oneThirdOfUpperControlLimit, input.oneThirdOfUpperControlLimit, settings.hiddenSeries, false).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.oneThirdOfUpperControlLimit, input.oneThirdOfUpperControlLimit, settings.hiddenSeries, false).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.twoThirdsOfUpperControlLimit,
				name: getLocalizedText('twoThirdsOfControlLimits', settings.locale),
				animation: settings.animation,
				data: input.twoThirdsOfUpperControlLimit,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.twoThirdsOfControlLimits,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 19,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.twoThirdsOfUpperControlLimit, input.twoThirdsOfUpperControlLimit, settings.hiddenSeries, false).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.twoThirdsOfUpperControlLimit, input.twoThirdsOfUpperControlLimit, settings.hiddenSeries, false).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.oneThirdOfLowerControlLimit,
				linkedTo: seriesIds.singleValueChart.oneThirdOfUpperControlLimit,
				//name: getLocalizedText('oneThirdOfLowerControlLimit', settings.locale), 
				animation: settings.animation,
				data: input.oneThirdOfLowerControlLimit,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.oneThirdOfControlLimits,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 20,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.twoThirdsOfLowerControlLimit,
				linkedTo: seriesIds.singleValueChart.twoThirdsOfUpperControlLimit,
				//name: getLocalizedText('twoThirdsOfLowerControlLimit', settings.locale),
				animation: settings.animation,
				data: input.twoThirdsOfLowerControlLimit,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.twoThirdsOfControlLimits,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 21,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.nominalValue,
				name: getLocalizedText('nominalValue', settings.locale), //  Highcharts.uiLocale[settings.locale].nominalValue,
				animation: settings.animation,
				data: input.nominalValue,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.nominalValue,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 22,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.nominalValue, input.nominalValue, settings.hiddenSeries, true).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.nominalValue, input.nominalValue, settings.hiddenSeries, true).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.upperMiddlethird,
				name: getLocalizedText('upperMiddlethird', settings.locale), //  Highcharts.uiLocale[settings.locale].upperMiddlethird
				animation: settings.animation,
				data: input.upperMiddlethird,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.upperMiddlethird,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 23,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.upperMiddlethird, input.upperMiddlethird, settings.hiddenSeries, false).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.upperMiddlethird, input.upperMiddlethird, settings.hiddenSeries, false).showInChart,
				events: {
					click: onSeriesClick
				}
			},
			{
				id: seriesIds.singleValueChart.lowerMiddlethird,
				name: getLocalizedText('lowerMiddlethird', settings.locale), //  Highcharts.uiLocale[settings.locale].lowerMiddlethird
				animation: settings.animation,
				data: input.lowerMiddlethird,
				yAxis: 0,
				color: chartSeriesColors.SingleValueChart.lowerMiddlethird,
				step: 'left',
				tooltip: {
					valueDecimals: settings.decimalPlaces
				},
				marker: {
					enabled: false
				},
				lineWidth: 1.2,
				legendIndex: 24,
				showInLegend: seriesVisibility(seriesIds.singleValueChart.lowerMiddlethird, input.lowerMiddlethird, settings.hiddenSeries, false).showInLegend,
				visible: seriesVisibility(seriesIds.singleValueChart.lowerMiddlethird, input.lowerMiddlethird, settings.hiddenSeries, false).showInChart,
				events: {
					click: onSeriesClick
				}
			}
		];
		return ser;
	}

	/**
	 * @description The series click callback event.
	 * @memberof SingleValueChart
	 * @function onSeriesClick
	 * @param {object} data - The object hold the information of clicked point.
	 * @param {number} data.sequenceId - The corresponding sequence id of the clicked point.
	 * @param {string} data.seriesName - The clicked series name.
	 * @param {number} data.point - The clicked point value
	 * @param {string} data.seriesId - The id of the clicked series.
	 * @example
	 * // When a callback is registered with options in single value chart options
	 *  $('#svc').singleValueChart(
	 *                  {
	 *                      locale: "en",
	 *                      data: {
	 *                          specification: {
	*                                   controlChartType : 'xb'
	*                               },
	 *                          measurements : [], // Array of objects in defined format
	 *                          subgroups: [],  // Array of objects in defined format
	 *                      },
	 *                      decimalPlaces: 2,
	 *                      hiddenSeries: ["lcl"],
	 *                      onSeriesClick: function (data) {
	 *                                          console.log(data);
	 *                                          //The output will be like this
	 *                                          // sequenceId: 484, seriesName: "measurements__1", point: 19.99, seriesId: "measurements__1"
	 *                                          }
	 *                   }
	 *                );
	 *

	 */
	function onSeriesClick(e) {
		if (typeof settings.onSeriesClick === 'function') { // if callback is defined
			var sequenceId,
				point,
				seriesID,
				seriesName,
				output,
				referenceId;

			if (e && e.point && typeof e.point.x === 'number' && settings.data.measurements[e.point.x]) {
				sequenceId = settings.data.measurements[e.point.x].sequenceID;
				referenceId = settings.data.measurements[e.point.x].referenceID;

				if (e.point.series) {
					seriesName = e.point.series.name;
					point = e.point.y;
					seriesID = e.point.series.options.id;
				}
			} else {
				if (e.point.series && e.point) {
					seriesName = e.point.series.name;
					point = e.point.y;
					seriesID = e.point.series.options.id;
				}
			}
			output = { sequenceId: sequenceId, seriesName: seriesName, point: point, seriesId: seriesID, referenceId: referenceId };
			settings.onSeriesClick(output);
		}
	}

	/**
	* @description The legend click callback event.
	* @memberof SingleValueChart
	* @function onLegendClick
	* @param {Object} data - The object hold the information of clicked legend.
	* @param {string} data.seriesId - The series id of the clicked legend item.
	* @param {boolean} data.isVisible - Flag to represent the state of series that whether series was visible before click event or not.

	* @example
	* // When a callback is registered with options in single value chart options
	*  $('#svc').singleValueChart(
	*                  {
	*                      locale: "en",
	*                      data: {
	*                          specification: {
	*                                   controlChartType : 'xb'
	*                               },
	*                          measurements : [], // Array of objects in defined format
	*                          subgroups: [],  // Array of objects in defined format
	*                      },
	*                      decimalPlaces: 2,
	*                      hiddenSeries: ["lcl"],
	*                      onLegendClick : function (data) {
	*                                          console.log(data);
	*                                            //The output will be like this
	*                                            // seriesId: "lcl", isVisible: false
	*                                       }
	*                  }
	*              );
	*
	*/
	function onLegendClick(e) {
		if (typeof settings.onLegendClick === 'function') {
			var seriesID,
				isVisible,
				output;
			seriesID = e.target.userOptions.id;
			isVisible = this.visible;
			output = { seriesId: seriesID, isVisible: isVisible };

			settings.onLegendClick(output);
		}
	}

	/**
	* @description The callback which is called when a chart is drawn. 
	* @memberof SingleValueChart
	* @callback onChartLoaded
	* @param {object} e - The event object with chart API object
	* @example
	* // When a callback is registered with options in single value chart options
	*  $('#svc').singleValueChart(
	*                  {
	*                      locale: "en",
	*                      data: {
	*                          specification: {
	*                                   controlChartType : 'xb'
	*                               },
	*                          measurements : [], // Array of objects in defined format
	*                          subgroups: [],  // Array of objects in defined format
	*                      },
	*                      decimalPlaces: 2,
	*                      onChartLoaded : function (data) {
	*                                          console.log(data);
	*                                       }
	*                  }
	*              );
	*/
	function onChartLoaded(e) {
		if (typeof settings.onChartLoaded === 'function') { // if callback is defined
			settings.onChartLoaded(e);
		}
	}


	function getCustomxAxisLabel(customInfo) {
		if (customInfo && customInfo.filter) {
			var xAxisObj = customInfo.filter(function (k) { return k.xAxisLabel === true; });
			if (xAxisObj && xAxisObj.length > 0 && xAxisObj[0].value)
				return xAxisObj[0].value;
		}
		return '';
	}

	function getCustomTooltip(customInfo) {
		var customTooltip = [];

		if (customInfo && customInfo.filter) {
			var xAxisObj = customInfo.filter(function (k) { return k.showInTooltip === true; });
			if (xAxisObj) {
				xAxisObj.forEach(function (o) {
					//customTooltip.push(o.label + ' : ' + o.value);
					customTooltip.push(tooltipRow.format(o.label, o.value, 'customInfo'));
				});
			}
		}
		return customTooltip.join('');
	}

	function getCombinedTooltip() {
		var msg = [],
			msgTr = [],
			customInfo = settings.inputs.customInfo || [];

		if (this.points && this.points.length > 0) {
			var violationPoint = null,
			point = this.points[0].point;
			/*
			ucl: 'ucl',
			lcl: 'lcl',
			uwl: 'uwl',
			lwl: 'lwl',
			utl: 'utl',
			ltl: 'ltl',
			xbp: 'xbp',
			measurement: 'measurements__1',
			flagSeries: 'flagSeries'

			firstChoiceUpperLimit: [], 
			firstChoiceLowerLimit: [], 
			secondChoiceUpperLimit: [], 
			secondChoiceLowerLimit: [], 
			thirdChoiceUpperLimit: [], 
			thirdChoiceLowerLimit: [],
			centerOfTolerances: [], 
			averageOfAllValues: [], 
			oneThirdOfUpperControlLimit: [], 
			twoThirdsOfLowerControlLimit: [],
			nominalValue: []
			 **/
			var ucl = findSeries(this.points, seriesIds.singleValueChart.ucl),
				lcl = findSeries(this.points, seriesIds.singleValueChart.lcl),
				uwl = findSeries(this.points, seriesIds.singleValueChart.uwl),
				lwl = findSeries(this.points, seriesIds.singleValueChart.lwl),
				utl = findSeries(this.points, seriesIds.singleValueChart.utl),
				ltl = findSeries(this.points, seriesIds.singleValueChart.ltl),
				xbp = findSeries(this.points, seriesIds.singleValueChart.xbp),
				measurement = findSeries(this.points, seriesIds.singleValueChart.measurement),
				additionalMeasurement = findSeries(this.points, seriesIds.singleValueChart.additionalMeasurement),

				firstChoiceUpperLimit = findSeries(this.points, seriesIds.singleValueChart.firstChoiceUpperLimit),
				tt_firstChoiceUpperLimit = getLocalizedText('tt_firstChoiceUpperLimit', settings.locale),
				firstChoiceLowerLimit = findSeries(this.points, seriesIds.singleValueChart.firstChoiceLowerLimit),
				tt_firstChoiceLowerLimit = getLocalizedText('tt_firstChoiceLowerLimit', settings.locale),
				secondChoiceUpperLimit = findSeries(this.points, seriesIds.singleValueChart.secondChoiceUpperLimit),
				tt_secondChoiceUpperLimit = getLocalizedText('tt_secondChoiceUpperLimit', settings.locale),
				secondChoiceLowerLimit = findSeries(this.points, seriesIds.singleValueChart.secondChoiceLowerLimit),
				tt_secondChoiceLowerLimit = getLocalizedText('tt_secondChoiceLowerLimit', settings.locale),
				thirdChoiceUpperLimit = findSeries(this.points, seriesIds.singleValueChart.thirdChoiceUpperLimit),
				tt_thirdChoiceUpperLimit = getLocalizedText('tt_thirdChoiceUpperLimit', settings.locale),
				thirdChoiceLowerLimit = findSeries(this.points, seriesIds.singleValueChart.thirdChoiceLowerLimit),
				tt_thirdChoiceLowerLimit = getLocalizedText('tt_thirdChoiceLowerLimit', settings.locale),
				centerOfTolerances = findSeries(this.points, seriesIds.singleValueChart.centerOfTolerances),
				averageOfAllValues = findSeries(this.points, seriesIds.singleValueChart.averageOfAllValues),

				oneThirdOfUpperControlLimit = findSeries(this.points, seriesIds.singleValueChart.oneThirdOfUpperControlLimit),
				tt_oneThirdOfControlLimitsUpper = getLocalizedText('tt_oneThirdOfControlLimitsUpper', settings.locale),

				oneThirdOfLowerControlLimit = findSeries(this.points, seriesIds.singleValueChart.oneThirdOfLowerControlLimit),
				tt_oneThirdOfControlLimitsLower = getLocalizedText('tt_oneThirdOfControlLimitsLower', settings.locale),

				twoThirdsOfUpperControlLimit = findSeries(this.points, seriesIds.singleValueChart.twoThirdsOfUpperControlLimit),
				tt_twoThirdsOfControlLimitsUpper = getLocalizedText('tt_twoThirdsOfControlLimitsUpper', settings.locale),

				twoThirdsOfLowerControlLimit = findSeries(this.points, seriesIds.singleValueChart.twoThirdsOfLowerControlLimit),
				tt_twoThirdsOfControlLimitsLower = getLocalizedText('tt_twoThirdsOfControlLimitsLower', settings.locale),

				nominalValue = findSeries(this.points, seriesIds.singleValueChart.nominalValue),
				tt_NominalValue = getLocalizedText('tt_NominalValue', settings.locale),

				lowerMiddlethird = findSeries(this.points, seriesIds.singleValueChart.lowerMiddlethird),
				upperMiddlethird = findSeries(this.points, seriesIds.singleValueChart.upperMiddlethird)

			msg.push(tooltipTableStart);
			msgTr.push(tooltipTableStart);

			if (measurement || additionalMeasurement) {
				if (measurement) {
					msgTr.push(tooltipRow.format('<b>' + getLocalizedText('SVC_yAxisTitle', settings.locale) + '</b>', '<b>' + roundFloat(measurement.y, settings.decimalPlaces, false) + '</b>', measurement.series.userOptions.id));
					msg.push(tooltipRow.format('<b>' + getLocalizedText('SVC_yAxisTitle', settings.locale) + '</b>', '<b>' + roundFloat(measurement.y, settings.decimalPlaces, false) + '</b>', measurement.series.userOptions.id));
				}
				if (additionalMeasurement) {
					var mValue = settings.data.additionalMeasurements[this.x];
					msgTr.push(tooltipRow.format('<b>' + getLocalizedText('AdditionalMeasurement', settings.locale) + '</b>', '<b>' + roundFloat(mValue, settings.decimalPlaces, false) + '</b>', 'AdditionalMeasurement'));
					msg.push(tooltipRow.format('<b>' + getLocalizedText('AdditionalMeasurement', settings.locale) + '</b>', '<b>' + roundFloat(mValue, settings.decimalPlaces, false) + '</b>', 'AdditionalMeasurement'));
				}
				msg.push(tooltipHr);
			}           

			if (utl) {
				msg.push(tooltipRow.format(utl.series.name, roundFloat(utl.y, settings.decimalPlaces, false), utl.series.userOptions.id));
				msgTr.push(tooltipRow.format(utl.series.name, roundFloat(utl.y, settings.decimalPlaces, false), utl.series.userOptions.id));
			}

			if (ucl) {
				msg.push(tooltipRow.format(ucl.series.name, roundFloat(ucl.y, settings.decimalPlaces, false), ucl.series.userOptions.id));
				msgTr.push(tooltipRow.format(ucl.series.name, roundFloat(ucl.y, settings.decimalPlaces, false), ucl.series.userOptions.id));
			}

			if (uwl) {
				msg.push(tooltipRow.format(uwl.series.name, roundFloat(uwl.y, settings.decimalPlaces, false), uwl.series.userOptions.id));
				msgTr.push(tooltipRow.format(uwl.series.name, roundFloat(uwl.y, settings.decimalPlaces, false), uwl.series.userOptions.id));
			}

			if (xbp) {
				msg.push(tooltipRow.format(xbp.series.name, roundFloat(xbp.y, settings.decimalPlaces, false), xbp.series.userOptions.id));
				msgTr.push(tooltipRow.format(xbp.series.name, roundFloat(xbp.y, settings.decimalPlaces, false), xbp.series.userOptions.id));
			}

			if (lwl) {
				msg.push(tooltipRow.format(lwl.series.name, roundFloat(lwl.y, settings.decimalPlaces, false), lwl.series.userOptions.id));
				msgTr.push(tooltipRow.format(lwl.series.name, roundFloat(lwl.y, settings.decimalPlaces, false), lwl.series.userOptions.id));
			}

			if (lcl) {
				msg.push(tooltipRow.format(lcl.series.name, roundFloat(lcl.y, settings.decimalPlaces, false), lcl.series.userOptions.id));
				msgTr.push(tooltipRow.format(lcl.series.name, roundFloat(lcl.y, settings.decimalPlaces, false), lcl.series.userOptions.id));
			}

			if (ltl) {
				msg.push(tooltipRow.format(ltl.series.name, roundFloat(ltl.y, settings.decimalPlaces, false), ltl.series.userOptions.id));
				msgTr.push(tooltipRow.format(ltl.series.name, roundFloat(ltl.y, settings.decimalPlaces, false), ltl.series.userOptions.id));
			}

			// ==============================
			if (firstChoiceUpperLimit) {
				msg.push(tooltipRow.format(tt_firstChoiceUpperLimit, roundFloat(firstChoiceUpperLimit.y, settings.decimalPlaces, false), firstChoiceUpperLimit.series.userOptions.id));
				msgTr.push(tooltipRow.format(tt_firstChoiceUpperLimit, roundFloat(firstChoiceUpperLimit.y, settings.decimalPlaces, false), firstChoiceUpperLimit.series.userOptions.id));

				msg.push(tooltipRow.format(tt_firstChoiceLowerLimit, roundFloat(firstChoiceLowerLimit.y, settings.decimalPlaces, false), firstChoiceLowerLimit.series.userOptions.id));
				msgTr.push(tooltipRow.format(tt_firstChoiceLowerLimit, roundFloat(firstChoiceLowerLimit.y, settings.decimalPlaces, false), firstChoiceLowerLimit.series.userOptions.id));
			}
			if (secondChoiceUpperLimit) {
				msg.push(tooltipRow.format(tt_secondChoiceUpperLimit, roundFloat(secondChoiceUpperLimit.y, settings.decimalPlaces, false), secondChoiceUpperLimit.series.userOptions.id));
				msgTr.push(tooltipRow.format(tt_secondChoiceUpperLimit, roundFloat(secondChoiceUpperLimit.y, settings.decimalPlaces, false), secondChoiceUpperLimit.series.userOptions.id));

				msg.push(tooltipRow.format(tt_secondChoiceLowerLimit, roundFloat(secondChoiceLowerLimit.y, settings.decimalPlaces, false), secondChoiceLowerLimit.series.userOptions.id));
				msgTr.push(tooltipRow.format(tt_secondChoiceLowerLimit, roundFloat(secondChoiceLowerLimit.y, settings.decimalPlaces, false), secondChoiceLowerLimit.series.userOptions.id));
			}
			if (thirdChoiceUpperLimit) {
				msg.push(tooltipRow.format(tt_thirdChoiceUpperLimit, roundFloat(thirdChoiceUpperLimit.y, settings.decimalPlaces, false), thirdChoiceUpperLimit.series.userOptions.id));
				msgTr.push(tooltipRow.format(tt_thirdChoiceUpperLimit, roundFloat(thirdChoiceUpperLimit.y, settings.decimalPlaces, false), thirdChoiceUpperLimit.series.userOptions.id));

				msg.push(tooltipRow.format(tt_thirdChoiceLowerLimit, roundFloat(thirdChoiceLowerLimit.y, settings.decimalPlaces, false), thirdChoiceLowerLimit.series.userOptions.id));
				msgTr.push(tooltipRow.format(tt_thirdChoiceLowerLimit, roundFloat(thirdChoiceLowerLimit.y, settings.decimalPlaces, false), thirdChoiceLowerLimit.series.userOptions.id));
			}
			if (centerOfTolerances) {
				msg.push(tooltipRow.format(centerOfTolerances.series.name, roundFloat(centerOfTolerances.y, settings.decimalPlaces, false), centerOfTolerances.series.userOptions.id));
				msgTr.push(tooltipRow.format(centerOfTolerances.series.name, roundFloat(centerOfTolerances.y, settings.decimalPlaces, false), centerOfTolerances.series.userOptions.id));
			}
			if (averageOfAllValues) {
				msg.push(tooltipRow.format(averageOfAllValues.series.name, roundFloat(averageOfAllValues.y, settings.decimalPlaces, false), averageOfAllValues.series.userOptions.id));
				msgTr.push(tooltipRow.format(averageOfAllValues.series.name, roundFloat(averageOfAllValues.y, settings.decimalPlaces, false), averageOfAllValues.series.userOptions.id));
			} 
			if (oneThirdOfUpperControlLimit) {
				msg.push(tooltipRow.format(tt_oneThirdOfControlLimitsUpper, roundFloat(oneThirdOfUpperControlLimit.y, settings.decimalPlaces, false), oneThirdOfUpperControlLimit.series.userOptions.id));
				msgTr.push(tooltipRow.format(tt_oneThirdOfControlLimitsUpper, roundFloat(oneThirdOfUpperControlLimit.y, settings.decimalPlaces, false), oneThirdOfUpperControlLimit.series.userOptions.id));

				msg.push(tooltipRow.format(tt_oneThirdOfControlLimitsLower, roundFloat(oneThirdOfLowerControlLimit.y, settings.decimalPlaces, false), oneThirdOfLowerControlLimit.series.userOptions.id));
				msgTr.push(tooltipRow.format(tt_oneThirdOfControlLimitsLower, roundFloat(oneThirdOfLowerControlLimit.y, settings.decimalPlaces, false), oneThirdOfLowerControlLimit.series.userOptions.id));
			}
			if (twoThirdsOfUpperControlLimit) {
				msg.push(tooltipRow.format(tt_twoThirdsOfControlLimitsUpper, roundFloat(twoThirdsOfUpperControlLimit.y, settings.decimalPlaces, false), twoThirdsOfUpperControlLimit.series.userOptions.id));
				msgTr.push(tooltipRow.format(tt_twoThirdsOfControlLimitsUpper, roundFloat(twoThirdsOfUpperControlLimit.y, settings.decimalPlaces, false), twoThirdsOfUpperControlLimit.series.userOptions.id));

				msg.push(tooltipRow.format(tt_twoThirdsOfControlLimitsLower, roundFloat(twoThirdsOfLowerControlLimit.y, settings.decimalPlaces, false), twoThirdsOfLowerControlLimit.series.userOptions.id));
				msgTr.push(tooltipRow.format(tt_twoThirdsOfControlLimitsLower, roundFloat(twoThirdsOfLowerControlLimit.y, settings.decimalPlaces, false), twoThirdsOfLowerControlLimit.series.userOptions.id));
			}
			
			if (nominalValue) {
				msg.push(tooltipRow.format(tt_NominalValue, roundFloat(nominalValue.y, settings.decimalPlaces, false), nominalValue.series.userOptions.id));
				msgTr.push(tooltipRow.format(tt_NominalValue, roundFloat(nominalValue.y, settings.decimalPlaces, false), nominalValue.series.userOptions.id));
			}
			if (upperMiddlethird) {
				msg.push(tooltipRow.format(upperMiddlethird.series.name, roundFloat(upperMiddlethird.y, settings.decimalPlaces, false), upperMiddlethird.series.userOptions.id));
				msgTr.push(tooltipRow.format(upperMiddlethird.series.name, roundFloat(upperMiddlethird.y, settings.decimalPlaces, false), upperMiddlethird.series.userOptions.id));
			}
			if (lowerMiddlethird) {
				msg.push(tooltipRow.format(lowerMiddlethird.series.name, roundFloat(lowerMiddlethird.y, settings.decimalPlaces, false), lowerMiddlethird.series.userOptions.id));
				msgTr.push(tooltipRow.format(lowerMiddlethird.series.name, roundFloat(lowerMiddlethird.y, settings.decimalPlaces, false), lowerMiddlethird.series.userOptions.id));
			}
			
			//if (utl || ucl || uwl || xbp || lwl || lcl || ltl || firstChoiceUpperLimit || secondChoiceUpperLimit || thirdChoiceUpperLimit || centerOfTolerances || averageOfAllValues || oneThirdOfUpperControlLimit || twoThirdsOfUpperControlLimit)
			//    msg.push(tooltipHr);


			$.each(this.points, function (i, ser) {
				if (ser.key.toString().startsWith('violation') || ser.key.toString().startsWith('outlier') || ser.key.toString().startsWith('eliminated')) {
					violationPoint = ser;
				}
			});

			if (settings.inputs.customInfo.length > point.index && customInfo[point.index].length > 0) {
				if (utl || ucl || uwl || xbp || lwl || lcl || ltl || firstChoiceUpperLimit || secondChoiceUpperLimit || thirdChoiceUpperLimit || centerOfTolerances || averageOfAllValues || oneThirdOfUpperControlLimit || twoThirdsOfUpperControlLimit || upperMiddlethird || lowerMiddlethird)
					msg.push(tooltipHr);

				msg.push(getCustomTooltip(settings.inputs.customInfo[point.index]));
				msgTr.push(getCustomTooltip(settings.inputs.customInfo[point.index]));
			}

			if (violationPoint && violationPoint.point.vType) {
				msg.push(tooltipHr);
				$.each(violationPoint.point.vType, function (i, violation) {
					if (violation !== 'processviolation_2' && violation !== 'processviolation_1' && violation !== 'remark') {
						msg.push(tooltipRow.format(getLocalizedText(violation, settings.locale), "", 'violation'));
						msgTr.push(tooltipRow.format(getLocalizedText(violation, settings.locale), "", "violation"));
					}
				});

			}

			msg.push(tooltipTableEnd);

		} else if (this.series && this.series.name === seriesIds.singleValueChart.flagSeries) {
			msg = [];
			msg.push(getCustomRemarks(settings, this.point.index));
		}

		if (settings.series && settings.series.tooltip && settings.series.tooltip.length > 0 && this.points && this.points.length > 0) {
			msg = applySeriesOrder(msgTr);
		}

		return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + msg.join('') + "</span></div>";
	}

	function displayTooltip(/*tooltip*/) {
		// single-value-chart-tooltip

		var msg = [],
			sequenceIds = settings.inputs.sequenceIds ? settings.inputs.sequenceIds : [],
			customInfo = settings.inputs.customInfo || [];

		msg.push(tooltipTableStart);
		if (settings.splitTooltip === false) {
			return getCombinedTooltip.bind(this)();
		}
		msg.push(tooltipRow.format('<b>' + getLocalizedText('SVC_xAxisTitle', settings.locale) + '</b>', '<b>' + sequenceIds[this.point.index] + '</b>'));

		if (this.series.name === seriesIds.singleValueChart.measurement)
			msg.push(tooltipRow.format('<b>' + getLocalizedText('SVC_yAxisTitle', settings.locale) + '</b>', '<b>' + roundFloat(this.y, settings.decimalPlaces) + '</b>'));
		else
			msg.push(tooltipRow.format('<b>' + this.series.name + ' </b> ', '<b>' + roundFloat(this.y, settings.decimalPlaces) + '</b>'));

		if (settings.inputs.customInfo.length > this.point.index && customInfo[this.point.index].length > 0) {
			msg.push(tooltipHr);
			msg.push(getCustomTooltip(settings.inputs.customInfo[this.point.index]));
		}

		if (this.key.toString().startsWith('violation') || this.key.toString().startsWith('outlier') || this.key.toString().startsWith('eliminated')) {
			if (this.point && this.point.vType) {
				msg.push('<hr style="margin-top: 7px; padding: 0; margin-bottom: -10px; "/>');
				$.each(this.point.vType, function (i, violation) {
					if (violation !== 'processviolation_2' && violation !== 'processviolation_1' && violation !== 'remark') {
						msg.push(tooltipHr);
						msg.push(tooltipRow.format(getLocalizedText(violation, settings.locale), ""));
					}
				});
			}
		} else if (this.series.name === seriesIds.singleValueChart.flagSeries && this.point) {
			// msg = [this.point.text]; // just show annotation
			msg = [];
			msg.push(getCustomRemarks(settings, this.point.x));
			// else return tooltip.defaultFormatter.call(this, tooltip);
		}
		if (this.point.text) // for flags type series, only it should be shown
			return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + this.point.text + "</span></div>";
		msg.push(tooltipTableEnd);

		var html = '<div style="padding:8px 16px"><span style="font-size:9pt;font-weight:400">' + msg.join('') + '</span></div>';
		//return tooltip.defaultFormatter.call(this, tooltip);
		return html;
	}

	//function shouldDisplaySeries(series, hiddenSeriesId) {
	//    var notNullSeries = [];
	//    if (series && series.filter) {
	//        notNullSeries = series.filter(function (k) {
	//            if (k !== null && k !== undefined)
	//                return k.toString();
	//        });
	//    }

	//    var nonullableSeries = notNullSeries.length > 0;
	//    var isVisibleSeries = !(settings.hiddenSeries.indexOf(hiddenSeriesId) > -1);

	//    return nonullableSeries && isVisibleSeries;
	//}

	function setDefaults(_options) {
		var opt = undefined;
		if (_options && typeof _options === 'object') {
			//_options.siemensTooltip = $.extend({}, siemensTooltip);
			//_options.siemensTooltip.formatter = displayTooltip;
			opt = $.extend({}, _options);

			opt.chartType = chartTypes.svc; // cannot be changed.
			opt.siemensTooltip = $.extend({}, siemensTooltip);
			opt.siemensTooltip.formatter = displayTooltip;
			if (!opt.locale || !Highcharts.uiLocale[opt.locale]) {
				console.error('Locale ' + opt.locale + ' not found, default English locales will be used');
				opt.locale = 'en';
			}
			if (typeof opt.yAxisUnit !== 'object') opt.yAxisUnit = getDefaultyAxisUnitObj();
			else opt.yAxisUnit = $.extend(getDefaultyAxisUnitObj(), opt.yAxisUnit);
			if (_options.splitTooltip === false/* && _options.siemensTooltip*/) {
				opt.siemensTooltip.split = true; // will combined the tooltip in highstock
			}

			if (_options.fontSize && _options.fontSize.labels) {
				axiesTitleStyle.fontSize = _options.fontSize.labels;
			}
			if (_options.fontSize && _options.fontSize.title) {
				titleStyle.fontSize = _options.fontSize.title;
			}

			if (_options.titleSettings) {
				_options.titleSettings.titleStyle = $.extend({}, titleStyle);
				_options.titleSettings.titleStyle.transform = 'translate(' + _options.titleSettings.x + 'px, ' + _options.titleSettings.y + 'px) rotate(' + _options.titleSettings.rotate + 'deg)'
			}

			if (!opt.series && typeof opt.series !== 'object') opt.series = getDefaultSeriesObj();
			else opt.series = $.extend(getDefaultSeriesObj(), opt.series);
		}

		return opt;
	}

	function chooseValues(primValue, secondValueArray) {
		return typeof primValue === 'number' ? primValue : secondValueArray[secondValueArray.length - 1] || null;
	}

	
	function addSeriesPoints(input) {

		// convert to float if number are passed as string.
		input.lcl = roundFloat(input.lcl, null, true);
		input.ucl = roundFloat(input.ucl, null, true);
		input.lwl = roundFloat(input.lwl, null, true);
		input.uwl = roundFloat(input.uwl, null, true);
		input.ltl = roundFloat(input.ltl, null, true);
		input.utl = roundFloat(input.utl, null, true);
		input.xbp = roundFloat(input.xbp, null, true);
		input.nominalValue = roundFloat(input.nominalValue, null, true);
		input.measurement = roundFloat(input.measurement, null, true);
		input.additionalMeasurement = roundFloat(input.additionalMeasurement, null, true);
		input.averageOfAllValues = roundFloat(input.averageOfAllValues, null, true);


		var chart = $this.data('chartApi'),
			// $this.data('settings', settings)
			inputs = $this.data('settings').inputs, // $this.data('inputData'),
			lcl = chooseValues(input.lcl, inputs.lcl),
			ucl = chooseValues(input.ucl, inputs.ucl),
			lwl = chooseValues(input.lwl, inputs.lwl),
			uwl = chooseValues(input.uwl, inputs.uwl),
			ltl = chooseValues(input.ltl, inputs.ltl),
			utl = chooseValues(input.utl, inputs.utl),
			xbp = chooseValues(input.xbp, inputs.xbp),
			nominalValue = chooseValues(input.nominalValue, inputs.nominalValue),
			averageOfAllValues = chooseValues(input.averageOfAllValues, inputs.averageOfAllValues),
			additionalMeasurement = chooseValues(input.additionalMeasurement, inputs.additionalMeasurements),
			measurement = input.measurement,
			subgrpNum = inputs.sortNumbers.length > 0 ? inputs.sortNumbers[inputs.sortNumbers.length - 1] + 1 : 0;

		if ($this.data('settings').data.additionalMeasurements && $this.data('settings').data.additionalMeasurements.length > 0 && additionalMeasurement) {
			$this.data('settings').data.additionalMeasurements.push(additionalMeasurement);
		}

		if (typeof measurement !== 'number') throw 'The measurement value is not a valid number';


		if (chart && input) {
			chart.get(seriesIds.singleValueChart.measurement).addPoint(measurement, false); // true will redraw
			chart.get(seriesIds.singleValueChart.additionalMeasurement).addPoint(input.additionalMeasurement, false); // true will redraw
			chart.get(seriesIds.singleValueChart.lcl).addPoint(lcl, false); // data should always be add irrespect of null of not
			chart.get(seriesIds.singleValueChart.ucl).addPoint(ucl, false);
			chart.get(seriesIds.singleValueChart.lwl).addPoint(lwl, false);
			chart.get(seriesIds.singleValueChart.uwl).addPoint(uwl, false);
			chart.get(seriesIds.singleValueChart.ltl).addPoint(ltl, false);
			chart.get(seriesIds.singleValueChart.utl).addPoint(utl, false);
			chart.get(seriesIds.singleValueChart.nominalValue).addPoint(nominalValue, false);
			chart.get(seriesIds.singleValueChart.averageOfAllValues).addPoint(averageOfAllValues, false);
			chart.get(seriesIds.singleValueChart.xbp).addPoint(xbp, false);

			var show = false;   //was needed in the past and may be needed in futuren
			if (show) {
				if (!getSeries(seriesIds.singleValueChart.lcl).visible && typeof lcl === 'number') {
					chart.get(seriesIds.singleValueChart.lcl).update(
						{
							visible: true,
							showInLegend: true
						}, false);
				}

				if (!getSeries(seriesIds.singleValueChart.ucl).visible && typeof ucl === 'number') {
					chart.get(seriesIds.singleValueChart.ucl).update(
						{
							visible: true,
							showInLegend: true
						}, false);
				}

				if (!getSeries(seriesIds.singleValueChart.lwl).visible && typeof lwl === 'number') {
					chart.get(seriesIds.singleValueChart.lwl).update(
						{
							visible: true,
							showInLegend: true
						}, false);
				}

				if (!getSeries(seriesIds.singleValueChart.uwl).visible && typeof uwl === 'number') {
					chart.get(seriesIds.singleValueChart.uwl).update(
						{
							visible: true,
							showInLegend: true
						}, false);
				}

				if (!getSeries(seriesIds.singleValueChart.ltl).visible && typeof ltl === 'number') {
					chart.get(seriesIds.singleValueChart.ltl).update(
						{
							visible: true,
							showInLegend: true
						}, false);
				}

				if (!getSeries(seriesIds.singleValueChart.utl).visible && typeof utl === 'number') {
					chart.get(seriesIds.singleValueChart.utl).update(
						{
							visible: true,
							showInLegend: true
						}, false);
				}

				if (!getSeries(seriesIds.singleValueChart.nominalValue).visible && typeof nominalValue === 'number') {
					chart.get(seriesIds.singleValueChart.nominalValue).update(
						{
							visible: true,
							showInLegend: true
						}, false);
				}

				if (!getSeries(seriesIds.singleValueChart.averageOfAllValues).visible && typeof averageOfAllValues === 'number') {
					chart.get(seriesIds.singleValueChart.averageOfAllValues).update(
						{
							visible: true,
							showInLegend: true
						}, false);
				}

				if (!getSeries(seriesIds.singleValueChart.xbp).visible && typeof xbp === 'number') {
					chart.get(seriesIds.singleValueChart.xbp).update(
						{
							visible: true,
							showInLegend: true
						}, false);
				}
			}

			inputs.customInfo.push(input.customInfo || []);

			//additionalLines
			if (settings.data.subgroups && settings.data.subgroups.length > 0 && settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines) {
				chart.get(seriesIds.singleValueChart.oneThirdOfLowerControlLimit).addPoint(settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines.oneThirdOfLowerControlLimit);
				chart.get(seriesIds.singleValueChart.oneThirdOfUpperControlLimit).addPoint(settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines.oneThirdOfUpperControlLimit);
				chart.get(seriesIds.singleValueChart.twoThirdsOfLowerControlLimit).addPoint(settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines.twoThirdsOfLowerControlLimit);
				chart.get(seriesIds.singleValueChart.twoThirdsOfUpperControlLimit).addPoint(settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines.twoThirdsOfUpperControlLimit);
				chart.get(seriesIds.singleValueChart.firstChoiceUpperLimit).addPoint(settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines.firstChoiceUpperLimit);
				chart.get(seriesIds.singleValueChart.firstChoiceLowerLimit).addPoint(settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines.firstChoiceLowerLimit);
				chart.get(seriesIds.singleValueChart.secondChoiceUpperLimit).addPoint(settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines.secondChoiceUpperLimit);
				chart.get(seriesIds.singleValueChart.secondChoiceLowerLimit).addPoint(settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines.secondChoiceLowerLimit);
				chart.get(seriesIds.singleValueChart.thirdChoiceUpperLimit).addPoint(settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines.thirdChoiceUpperLimit);
				chart.get(seriesIds.singleValueChart.thirdChoiceLowerLimit).addPoint(settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines.thirdChoiceLowerLimit);
				chart.get(seriesIds.singleValueChart.centerOfTolerances).addPoint(settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines.centerOfTolerances);
				chart.get(seriesIds.singleValueChart.upperMiddlethird).addPoint(settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines.upperMiddlethird);
				chart.get(seriesIds.singleValueChart.upperMiddlethird).addPoint(settings.data.subgroups[settings.data.subgroups.length - 1].additionalLines.lowerMiddlethird);

			}

			inputs.sortNumbers.push(subgrpNum);
			inputs.sequenceIds.push(subgrpNum);

			chart.redraw();

		}
	}

	function isSubequalMeasure(charttype) {
		var charttypes = ['x_ms', 'x_mr', 'mxb_ms', 'mxb_mr'];
		if (charttypes.indexOf(charttype.toLowerCase()) > -1)
			return true;
		return false;
	}

	function prepareXAxis(input) {
		var xAxis = {
			lineColor: siemensColors.PL_BLACK_22,
			lineWidth: 2,
			gridLineColor: siemensColors.PL_BLACK_22,
			title: {
				text: settings.xAxisTitle || getLocalizedText('SVC_xAxisTitle', settings.locale), // Highcharts.uiLocale[settings.locale].SVC_xAxisTitle,
				style: axiesTitleStyle
			},
			labels: {
				rotation: settings.xAxisLabelsRotation,
				style: {
					color: settings.xAxis.labels.color,
					fontSize: settings.xAxis.labels.fontSize
                },
				formatter: function () {
					return '<span style="font: ' + settings.xAxis.labels.font + '">' + input.sortNumbers[this.value] + '</span>'; ;
				}
			},
			min: settings.navigator.start,
			max: settings.navigator.end,
			range: settings.navigator.selected // select last 25 samples
		}
		return xAxis;
    }
	

	function prepareYAxis() {
		var unit = settings.yAxisUnit && isNullOrUndefined(settings.yAxisUnit.text) ? '' : settings.yAxisUnit.text;
		var yAxis =
			[
				{
					// tickInterval: 0.008,                    
					lineColor: siemensColors.PL_BLACK_22,
					lineWidth: 2,
					min: getYaxisMinScaling(),
					max: getYaxisMaxScaling(),
					opposite: false,
					labels: {
						style: {
							color: settings.yAxis.labels.color,
							fontSize: settings.yAxis.labels.fontSize
						},
						formatter: function () {
							return '<span style="font: ' + settings.yAxis.labels.font + '">' + roundFloat(this.value, settings.decimalPlaces) + '</span>';
						}
					},
					title: {
						text: settings.yAxisTitle || getLocalizedText('SVC_yAxisTitle', settings.locale), // Highcharts.uiLocale[settings.locale].SVC_yAxisTitle,
						style: axiesTitleStyle
					}
				},
				{// to show units of chart
					opposite: false,
					title: {
						reserveSpace: false,
						text: unit,
						align: 'high',
						rotation: 0,
						x: settings.yAxisUnit.x,
						y: settings.yAxisUnit.y,
						style: {
							fontSize: settings.yAxisUnit.fontSize,
							color: siemensColors.PLBlack4
						}
					}
				}

			];

		return yAxis;
	}

	function prepareChartOptions(input) {
		var options =
		{
			chart: {
				type: 'line',
				animation: settings.animation,
				height: settings.height,
				width: settings.width,
				style: {
					fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
				},
				events: {
					load: onChartLoaded
				}
			},
			credits: {
				enabled: false
			},
			plotOptions: {
				series: {
					animation: settings.animation,
					states: {
						inactive: {
							opacity: 0.8
						}
					},
					marker: {
						states: {
							hover: {
								enabled: false
							}
						}
					},
					events: {
						legendItemClick: onLegendClick
					}
				}
				//,
				//line: {
				//    events: {
				//        legendItemClick: onLegendClick
				//    }
				//}
			},
			exporting: {
				enabled: false
			},
			legend: {
				enabled: settings.legendSettings.enabled,
				align: settings.legendSettings.align,
				verticalAlign: settings.legendSettings.verticalAlign,
				layout: settings.legendSettings.layout,
				x: settings.legendSettings.x,
				y: settings.legendSettings.y,
				itemStyle: axiesTitleStyle
			},
			navigator: {
				baseSeries: 4,
				enabled: true, // to show or hide zoom level on x-axis
				xAxis: {
					labels: {
						formatter: function () {
							return this.value;
						}
						// enabled: true
					}
				},
				series: {
					lineColor: chartSeriesColors.SingleValueChart.Measurement
				}
			},
			rangeSelector: {
				enabled: false
				// selected: 5  // date, month year etc
			},
			title: {
				text: settings.chartTitle,
				align: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.align)) ? 'center' : settings.titleSettings.align,
				style: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.titleStyle)) ? titleStyle : settings.titleSettings.titleStyle,
			},
			tooltip: settings.siemensTooltip,

			xAxis: prepareXAxis(input),
			yAxis: prepareYAxis(),
			series: prepareSeries(input)
		};

		validateLegendPositioning(options.legend);

		return options;
	}

	function prepareColoredZone() {
		var zone = [];
		var violationZones = [];
		var interval;

		if (settings.data && settings.data.measurements) {
			var violatedSubgroupsNumber = [];

			$.each(settings.data.measurements, function (i, p) {
				var // subgroupSize = settings.data.specifications.subgroupSize > 0 ? settings.data.specifications.subgroupSize : 1,
					subgrpNum = isSubequalMeasure(settings.data.specifications.controlChartType) ? i : p.subgroupNumber, //  parseInt(i / subgroupSize), 
					statuses = settings.data.subgroups && settings.data.subgroups[subgrpNum] && settings.data.subgroups[subgrpNum].statuses ? getVoilations(settings.data.subgroups[subgrpNum].statuses) : [];
				if (statuses.indexOf("involvedbyother") > -1) {
					violatedSubgroupsNumber.push(roundFloat(p.sequenceID));
				}

			});

		}
		interval = getSequence(violatedSubgroupsNumber)
		violationZones = getColoredSeriesZone(interval, siemensColors.violationZone);
		violationZones.forEach(function (violationZone) {
			zone.push(violationZone);

		});
		return zone;


	}

	function getDefaultyAxisUnitObj() {
		var obj = {
			text: '',
			x: 60,
			y: undefined,
			fontSize: undefined
		};
		return obj;
	}

	function getMeasurmentStatuses(measurement, status) {
		var statuses = [];
		if (measurement && measurement.statuses && measurement.statuses.filter) {
			statuses = measurement.statuses.filter(function (s) {
				if (s.category && s.category.toLowerCase() === status) {
					return true;
				}
			});
		}

		return statuses;
	}

	function getCustomRemarks(settings, index) {
		var customTooltip = [];
		var customInfo = undefined;
		if (settings.data && settings.data.measurements && settings.data.measurements[index]) {
			customInfo = settings.data.measurements[index].customInfo;
		}

		if (customInfo) {
			var xAxisObj = customInfo.filter(function (k) { return k.isRemark === true; });
			if (xAxisObj) {
				xAxisObj.forEach(function (o) {
					customTooltip.push(o.value);
				});
			}
		}

		return getLocalizedText('annotation', settings.locale) + ' : ' + customTooltip.join(', ');
	}

	function getSeries(seriesId) {
		var chart = $this.data('chartApi')
		if (seriesId && chart && chart.get(seriesId)) {
			return chart.get(seriesId);
		}
		return {};
	}

	function findSeries(inputObject, seriesId) {
		var foundSeries = null;
		if (inputObject) {
			var foundObject = inputObject.filter(function (ser) {
				if (ser && ser.series.userOptions && ser.series.userOptions.id === seriesId)
					return ser;
				return null;
			});

			if (foundObject && foundObject.length > 0)
				foundSeries = foundObject[0];
		}
		return foundSeries;
	}

	function getYaxisMinScaling() {
		if (!isNullOrUndefined(settings.yAxisScale)) {
			if (!isNullOrUndefined(settings.yAxisScale.min))
				return settings.yAxisScale.min;
			if (!isNullOrUndefined(settings.yAxisScale.minScaling))
				return getYaxisMinValue(settings.yAxisScale.minScaling)

		}
		return null;
	}
	function getYaxisMaxScaling() {
		if (!isNullOrUndefined(settings.yAxisScale)) {
			if (!isNullOrUndefined(settings.yAxisScale.max))
				return settings.yAxisScale.max;
			if (!isNullOrUndefined(settings.yAxisScale.maxScaling))
				return getYaxisMaxValue(settings.yAxisScale.maxScaling)

		}
		return undefined;
	}
	function getYaxisMinValue(scalingType) {
		var minValue = null;
		switch (scalingType) {
			case scaling.controlLimit:
				var lcl = settings.inputs.lcl.reduce(function (a, b) {
					return Math.min(a, b);
				});
				var ucl = settings.inputs.ucl.reduce(function (a, b) {
					return Math.min(a, b);
				});
				minValue = Math.min(lcl, ucl);
				break;
			case scaling.toleranceLimit:
				var ltl = settings.inputs.ltl.reduce(function (a, b) {
					return Math.min(a, b);
				});
				var utl = settings.inputs.utl.reduce(function (a, b) {
					return Math.min(a, b);
				});
				minValue = Math.min(lcl, ucl);
				break;

		}
		return minValue;
	}
	function getYaxisMaxValue(scalingType) {
		var maxValue = null;
		switch (scalingType) {
			case scaling.controlLimit:
				var lcl = settings.inputs.lcl.reduce(function (a, b) {
					return Math.max(a, b);
				});
				var ucl = settings.inputs.ucl.reduce(function (a, b) {
					return Math.max(a, b);
				});
				maxValue = Math.max(lcl, ucl);
				break;
			case scaling.toleranceLimit:
				var ltl = settings.inputs.ltl.reduce(function (a, b) {
					return Math.max(a, b);
				});
				var utl = settings.inputs.utl.reduce(function (a, b) {
					return Math.max(a, b);
				});
				maxValue = Math.max(ltl, utl);
				break;

		}
		return maxValue;
	}

	function applySeriesOrder(toBeReorderd) {

		var seriesOrder = settings.series.tooltip || [];

		for (var i = 0; i < seriesOrder.length; i++) {
			var oldElemAtIndex = undefined,
				tooltipSetting = seriesOrder[i];
			
			toBeReorderd.some(function (a, index) {
				var seriesId = $(a).attr('series_id');
				if (seriesId === tooltipSetting.seriesId && seriesId !== 'customInfo' && seriesId !== 'violation') {
					oldElemAtIndex = index;
					return true;
				}
			});
			// GUPTA send parameter as string, therefore compare to false string
			if (!isNullOrUndefined(tooltipSetting.showInTooltip) && tooltipSetting.showInTooltip.toString() === "false" && oldElemAtIndex > -1) { // just compare to false and not with undefined and null
				toBeReorderd[oldElemAtIndex] = "";
			} else 
				if (tooltipSetting.order < toBeReorderd.length - 1 && oldElemAtIndex > -1) { // Table tag is already added, so length - 1
				var temp = toBeReorderd[tooltipSetting.order];
				toBeReorderd[tooltipSetting.order] = toBeReorderd[oldElemAtIndex];
				toBeReorderd[oldElemAtIndex] = temp;
			}
		}

		var customInfoIndex = undefined;

		toBeReorderd.some(function (a, index) {
			var seriesId = $(a).attr('series_id');
			if (seriesId === "customInfo") {
				customInfoIndex = index;
				return true;
			}
		});

		if (customInfoIndex > -1) {
			toBeReorderd.splice(customInfoIndex, 0, tooltipHr);
			customInfoIndex = undefined;
		}

		toBeReorderd.some(function (a, index) {
			var seriesId = $(a).attr('series_id');
			if (seriesId === "violation") {
				customInfoIndex = index;
				return true;
			}
		});

		if (customInfoIndex > -1) {
			toBeReorderd.splice(customInfoIndex, 0, tooltipHr);
			customInfoIndex = undefined;
		}
		
		toBeReorderd.push(tooltipTableEnd);

		return toBeReorderd;
	}

	function getDefaultSeriesObj() {
		var obj =
		{
			tooltip: []
		};

		return obj;
	}

	function addAdditionalLines(input, subgroup, hasSubgroupAdditionalLines) {
		// 1. set all values to null
		var firstChoiceUpperLimit = null;
		var firstChoiceLowerLimit = null;
		var secondChoiceUpperLimit = null;
		var secondChoiceLowerLimit = null;
		var thirdChoiceUpperLimit = null;
		var thirdChoiceLowerLimit = null;

		var centerOfTolerances = null;
		var averageOfAllValues = null;
		var oneThirdOfUpperControlLimit = null;
		var twoThirdsOfUpperControlLimit = null;
		var oneThirdOfLowerControlLimit = null;
		var twoThirdsOfLowerControlLimit = null;
		var upperMiddlethird = null;
		var lowerMiddlethird = null;

		//2. try to get value from global additional lines
		if (input.data.additionalLines) {
			firstChoiceUpperLimit = roundFloat(input.data.additionalLines.firstChoiceUpperLimit, null, true);
			firstChoiceLowerLimit = roundFloat(input.data.additionalLines.firstChoiceLowerLimit, null, true);
			secondChoiceUpperLimit = roundFloat(input.data.additionalLines.secondChoiceUpperLimit, null, true);
			secondChoiceLowerLimit = roundFloat(input.data.additionalLines.secondChoiceLowerLimit, null, true);
			thirdChoiceUpperLimit = roundFloat(input.data.additionalLines.thirdChoiceUpperLimit, null, true);
			thirdChoiceLowerLimit = roundFloat(input.data.additionalLines.thirdChoiceLowerLimit, null, true);

			centerOfTolerances = roundFloat(input.data.additionalLines.centerOfTolerances, null, true);
			averageOfAllValues = roundFloat(input.data.additionalLines.averageOfAllValues, null, true);
			oneThirdOfUpperControlLimit = roundFloat(input.data.additionalLines.oneThirdOfUpperControlLimit, null, true);
			twoThirdsOfUpperControlLimit = roundFloat(input.data.additionalLines.twoThirdsOfUpperControlLimit, null, true);
			oneThirdOfLowerControlLimit = roundFloat(input.data.additionalLines.oneThirdOfLowerControlLimit, null, true);
			twoThirdsOfLowerControlLimit = roundFloat(input.data.additionalLines.twoThirdsOfLowerControlLimit, null, true);
			upperMiddlethird = roundFloat(input.data.additionalLines.upperMiddlethird, null, true);
			lowerMiddlethird = roundFloat(input.data.additionalLines.lowerMiddlethird, null, true);
		}

		//3. use data from subgroup, if exists
		if (hasSubgroupAdditionalLines) {
			if (subgroup && subgroup.additionalLines) {
				if (subgroup.additionalLines.firstChoiceUpperLimit != undefined && subgroup.additionalLines.firstChoiceUpperLimit != null)
					firstChoiceUpperLimit = roundFloat(subgroup.additionalLines.firstChoiceUpperLimit, null, true);

				if (subgroup.additionalLines.firstChoiceLowerLimit != undefined && subgroup.additionalLines.firstChoiceLowerLimit != null)
					firstChoiceLowerLimit = roundFloat(subgroup.additionalLines.firstChoiceLowerLimit, null, true);

				if (subgroup.additionalLines.secondChoiceUpperLimit != undefined && subgroup.additionalLines.secondChoiceUpperLimit != null)
					secondChoiceUpperLimit = roundFloat(subgroup.additionalLines.secondChoiceUpperLimit, null, true);

				if (subgroup.additionalLines.secondChoiceLowerLimit != undefined && subgroup.additionalLines.secondChoiceLowerLimit != null)
					secondChoiceLowerLimit = roundFloat(subgroup.additionalLines.secondChoiceLowerLimit, null, true);

				if (subgroup.additionalLines.thirdChoiceUpperLimit != undefined && subgroup.additionalLines.thirdChoiceUpperLimit != null)
					thirdChoiceUpperLimit = roundFloat(subgroup.additionalLines.thirdChoiceUpperLimit, null, true);

				if (subgroup.additionalLines.thirdChoiceLowerLimit != undefined && subgroup.additionalLines.thirdChoiceLowerLimit != null)
					thirdChoiceLowerLimit = roundFloat(subgroup.additionalLines.thirdChoiceLowerLimit, null, true);

				if (subgroup.additionalLines.centerOfTolerances != undefined && subgroup.additionalLines.centerOfTolerances != null)
					centerOfTolerances = roundFloat(subgroup.additionalLines.centerOfTolerances, null, true);

				if (subgroup.additionalLines.oneThirdOfUpperControlLimit != undefined && subgroup.additionalLines.oneThirdOfUpperControlLimit != null)
					oneThirdOfUpperControlLimit = roundFloat(subgroup.additionalLines.oneThirdOfUpperControlLimit, null, true);

				if (subgroup.additionalLines.twoThirdsOfUpperControlLimit != undefined && subgroup.additionalLines.twoThirdsOfUpperControlLimit != null)
					twoThirdsOfUpperControlLimit = roundFloat(subgroup.additionalLines.twoThirdsOfUpperControlLimit, null, true);

				if (subgroup.additionalLines.oneThirdOfLowerControlLimit != undefined && subgroup.additionalLines.oneThirdOfLowerControlLimit != null)
					oneThirdOfLowerControlLimit = roundFloat(subgroup.additionalLines.oneThirdOfLowerControlLimit, null, true);

				if (subgroup.additionalLines.twoThirdsOfLowerControlLimit != undefined && subgroup.additionalLines.twoThirdsOfLowerControlLimit != null)
					twoThirdsOfLowerControlLimit = roundFloat(subgroup.additionalLines.twoThirdsOfLowerControlLimit, null, true);

				if (subgroup.additionalLines.upperMiddlethird != undefined && subgroup.additionalLines.upperMiddlethird != null)
					upperMiddlethird = roundFloat(subgroup.additionalLines.upperMiddlethird, null, true);

				if (subgroup.additionalLines.lowerMiddlethird != undefined && subgroup.additionalLines.lowerMiddlethird != null)
					lowerMiddlethird = roundFloat(subgroup.additionalLines.lowerMiddlethird, null, true);

				if (subgroup.additionalLines.averageOfAllValues != undefined && subgroup.additionalLines.averageOfAllValues != null)
					averageOfAllValues = roundFloat(input.data.additionalLines.averageOfAllValues, null, true);
			}
		}

		input.firstChoiceUpperLimit.push(firstChoiceUpperLimit);
		input.firstChoiceLowerLimit.push(firstChoiceLowerLimit);
		input.secondChoiceUpperLimit.push(secondChoiceUpperLimit);
		input.secondChoiceLowerLimit.push(secondChoiceLowerLimit);
		input.thirdChoiceUpperLimit.push(thirdChoiceUpperLimit);
		input.thirdChoiceLowerLimit.push(thirdChoiceLowerLimit);

		input.centerOfTolerances.push(centerOfTolerances);
		input.averageOfAllValues.push(averageOfAllValues);
		input.oneThirdOfUpperControlLimit.push(oneThirdOfUpperControlLimit);
		input.twoThirdsOfUpperControlLimit.push(twoThirdsOfUpperControlLimit);
		input.oneThirdOfLowerControlLimit.push(oneThirdOfLowerControlLimit);
		input.twoThirdsOfLowerControlLimit.push(twoThirdsOfLowerControlLimit);
		input.upperMiddlethird.push(upperMiddlethird);
		input.lowerMiddlethird.push(lowerMiddlethird);
		
	}

	function findSubgroupByNumber(subgroups, subgroupNumber) {
		var foundSubgroup = subgroups.currentSelectedSubgroupSelected || {};

		// don't iterate everytime to find subgroup 
		if (subgroupNumber !== foundSubgroup.subgroupNumber) {
			
				$.each(subgroups, function (i, s) {
					if (s && s.subgroupNumber === subgroupNumber) {
						foundSubgroup = s;						
						subgroups.currentSelectedSubgroupSelected = s;
						return false;
                    }
					
				return true;
			});
		}

		return foundSubgroup;
    }
};
/**
 * @description Provide methods for drawing Probability Plot.
 *
 * @namespace ProbabilityPlot
 * */
/**
 * @description A new Probability Plot  can be drawn using this method.
 * @memberof ProbabilityPlot
 * @function probabilityPlot
 * @requires highstock.js
 * @requires jQuery.js
 * @param {ProbabilityPlot.options} options - The chart options parameter
 * @tutorial CreatepNetlCharts
 * @example
 * $('#pnet').probabilityPlot({
 *                  locale: 'en',
 *                  data: data,
 *                  decimalPlaces: 4,
 *                  height: 600,
 *                  width: undefined
 *                  });
 */
/**
* @memberof ProbabilityPlot
* @typedef {Object} options
* @property {string}  [locale=en]  User locale to display label and violations in a specific language
* @property {ProbabilityPlot.data} data  JSON Array with subgroup
* @property {number} [decimalPlaces=4] - Number of decimal places
* @property {number}  [height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
*                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
* @property {number}  [width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
* @property {ProbabilityPlot.yAxisUnit} [yAxisUnit] - The unit of y Axis if the parameter is not provided the title would be loaded from the corresponding localization.
* @property {string} [xAxisTitle='Measurement'] - The label for x-Axis if the parameter is not provided the title would be loaded from the corresponding localization.
* @property {string} [yAxisTitle=' '] - The label for y-Axis.
* @property {string} chartTitle The title of the chart if no value provided the corresponding localization value would be displayed 
* @property {string} Set the Chart Title if the parameter is not provided the title would be loaded from the corresponding localization. 
* @property {boolean}  [animation=false] -Enable or disable the initial animation
* @property {ProbabilityPlot.onChartLoaded} [onChartLoaded=null] - A callback can be register which will be invoked after chart is data is loaded and it is going to render. 
* @property {ProbabilityPlot.titleSettings} [options.titleSettings]  - Provides options to position & rotate the chart title
* @property {ProbabilityPlot.fontSize} [fontSize]-Option to set Fontsize of label & title.
* @property {number} [xAxisLabelsRotation] Rotation of the labels in degrees. When undefined, the engine will try to hide/autorotate lables to avoid overlapping.
* @property {ProbabilityPlot.correlationCoefficient} [correlationCoefficient] Defines the Probability plot correlation coefficient settings such as label text font size and font color 
*/
/**
 * @description Defines the Probability plot correlation coefficient settings such as label text font size and font color
 * @typedef {object} correlationCoefficient
 * @memberof ProbabilityPlot
 * @property {boolean} active=true if set to false the correlation coefficient info will not be displayed in the probability plot
 * @property {string} [labelText] The text used as a label for the correlation coefficient if not provided the corresponding localization will be used
 * @property {Position} [position] defines the x, y position of the correlation coefficient text on the chart
 * @property {string} [fontSize='13px'] the font size of the correlation coefficient text 
 * @property {string} [color='#666666'] the font color of the correlation coefficient text 
 */
/**
 * @typedef {Object} Position
 * @property {number} x - The x-coordinate of the position on the chart.
 * @property {number} y - The y-coordinate of the position on the chart.
 */
/**
 * @description The titleSettings object is to determine the position and rotation of the chart title
 * @typedef {object} titleSettings
 * @memberof ProbabilityPlot
 * @property {string} [align = undefined] - The horizontal alignment of the title. Can be one of "left", "center" and "right".
 * @property {number} [rotate = undefined] - The rotation of the text in degrees. 0 is horizontal, 270 is vertical reading from bottom to top.
 * @property {number} [x = undefind] - The x position of the title relative to the alignment
 * @property {number} [y = undefind] - The y position of the title relative to the alignment
 * @example
 *titleSettings:{
 *  align: 'left',
 *  rotate: '90',
 *  x: '35',
 *  y:'207'
 *},
 */
/**
 * @description The data array of the Probability plot
 * @typedef {object} data
 * @memberof ProbabilityPlot
 * @example
 *
 *  data : {
 *          measurements : [
 *                          {value: -1, yCoordinate: 16.27632592097139},
 *                          {value: -1, yCoordinate: 20.15157013521245}
 *                         ],
 *          yAxis :     [
 *                          {value: 3.510946297538803, name: "0,01"},
 *                          {value: 20.91518334301292, name: "1"}
 *                      ],
 *          xMinValue: -2,
 *          xMinValueWithoutTolerances: -2,
 *          xMaxValue: 1,
 *          xMaxValueWithoutTolerances: 1,
 *          resultLineY1: 0,
 *          resultLineY1WithoutTolerances: 0,
 *          resultLineY2: 100,
 *          resultLineY2WithoutTolerances: 100,
 *          resultLineX1: -1.5946437246673486,
 *          resultLineX1WithoutTolerances: -1.5946437246673486,
 *          resultLineX2: 0.7927497249172357,
 *          resultLineX2WithoutTolerances: 0.7927497249172357,
 *          xAxisIsLogarithmic: true,
 *          correlationCoefficient: 0.9826515831649699,
 *          gradient: 41.88668609164542,
 *          intercept: 66.79434112315349,
 *          showProbabilityPlotTolerances: true,
 *          showProbabilityPlotSLines: true,
 *          currentUpperToleranceLimitAbs: 0.6989700043360189,
 *          currentLowerToleranceLimitAbs: 0.654353443360189,
 *          p99: 0.4944192130104312,
 *          p0_13: -1.2963132127854518,
 *   }
 */
/**
 * @typedef {object} yAxisUnit
 * @memberof ProbabilityPlot
 * @description To specify the series yAxis unit related properties.
 * @property {string} [text=""] - To specify the series colors.
 * @property {number} [x=60] - x position of the text.
 * @property {number} [y=-6] - y position of the text.
 * @property {number|string} [fontSize=null] - The font size of the text. If as number will be given then unit will be in pixel. As string in points can be size given e.g. '12pt'
 * @example
 * $('#pnet').probabilityPlot({
 *                          locale: 'en',
 *                          data: data,
 *                          decimalPlaces: 4,
 *                          height: 600,
 *                          width: undefined,
 *                          yAxisUnit: {
 *                              text: 'DW05Unit01',
 *                              x: 100,
 *                              y: -20,
 *                              fontSize: '20pt'
 *                          }
 *              }); 
 */ 
/**
 * @typedef {object} fontSize
 * @memberof ProbabilityPlot
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */
$.fn.probabilityPlot = function (options) {
    setDefaults();
    var settings = {};
    var upperTolerancePlotLine;
    var lowerTolerancePlotLine;

    if (options && typeof options === 'object') {
        settings = $.extend({
            locale: 'en',
            data: {},
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            yAxisUnit: getDefaultyAxisUnitObj(),
            xAxisTitle: undefined,
            yAxisTitle: undefined,
            onChartLoaded: undefined,
            chartTitle: undefined,
            titleSettings: undefined,
            animation: false,
            xAxisLabelsRotation: undefined,
            correlationCoefficient: {
                active:true,
                labelText: getLocalizedText('pNet_correlationCoefficient', options.locale),
                position: {
                    x: 185,
                    y:125
                },
                fontSize: '13px',
                color: '#666666'
            }

        }, options);
    } else if (typeof options === 'string') { // Method call
        settings = $(this).data('settings') || {};  // restore the settings object from prev settings
    };
    var $this = this,
        _args = arguments,
        //chartOption = createChartOptions(settings),
        chart = {
            init: function () {
                var containerId = $this.attr('id');
                var chartOption = createChartOptions(settings);
                var _chart =  Highcharts.chart(containerId, chartOption);
                $this.data('chartApi', _chart);
                $this.data('settings', settings);
            },
            /**
            *  @memberof ProbabilityPlot
            *  @function changeToleranceLinesVisibility
            *  @description Toggles between visible and unvisible tolerance lines
            *
            *  @example
            *  //to change visibility of the tolerance lines
            *  $('#pnet').probabilityPlot('changeToleranceLinesVisibility');
            *  // this changes the tolerance lines from visible to unvisible or from unvisible to visible depending on the current state.
            *
            */
            changeToleranceLinesVisibility: function () {
                toggleToleranceLineVisibility();
            },

            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof ProbabilityPlot
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered.
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#probabilityPlot').probabilityPlot('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64',
            *                                           width : 1800,
            *                                           height: 600,
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        };
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });
    /**
* @description The callback which is called when a probability plot is drawing. 
* @memberof ProbabilityPlot
* @callback onChartLoaded
* @param {object} e - The event object with chart API object
* @example
* $('#pnet').probabilityPlot({
*                           locale: 'en',
*                           data: data,
*                           decimalPlaces: 4,
*                           height: 600,
*                           width: undefined,
*                           onChartLoaded : function (data) {
*                                          console.log(data);
*                           }
*                  });
*/
    function onChartLoaded(e) {
        if (typeof settings.onChartLoaded === 'function') { // if callback is defined
            settings.onChartLoaded(e);
        }
    }

    function createChartOptions(settings) {
        var dataObj = PrepareData(),
            siemens_tooltip = $.extend({}, siemensTooltip);
        var optionValues = {
            chart: {
                height: settings.height,
                animation: settings.animation,
                width: settings.width,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                },
                events: {
                    load: onChartLoaded,
                    render: renderCorrelationCoefficientText
                
                }
            },
            title: {
                text: settings.chartTitle || getLocalizedText('pNet_chart', options.locale),
                align: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.align)) ? 'center' : settings.titleSettings.align,
                style: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.titleStyle)) ? titleStyle : settings.titleSettings.titleStyle,
            },
            credits: {
                enabled: false
            },
            plotOptions: {
                series: {
                    animation: settings.animation,
                }
            },
            tooltip: siemens_tooltip, // siemensTooltip,
            yAxis: prepareYAxis(dataObj),
            xAxis: {
                title: {
                    text: settings.xAxisTitle || getLocalizedText("pNet_xAxisTitle", options.locale),
                    style: axiesTitleStyle
                },
                plotLines: dataObj.yPlotLines,
                max: dataObj.xMaxValue,
                min: dataObj.xMinValue,
                labels: {
                    rotation: settings.xAxisLabelsRotation,
                    formatter: function () {
                        if (settings.data.result.probabilityPlot.xAxisIsLogarithmic) {
                            return roundFloat(Math.pow(10, this.value), settings.decimalPlaces, false);
                        }

                        else
                            return this.value;
                    }
                },
                tickWidth: 1,
                gridLineColor: siemensColors.PL_BLACK_22,
                gridLineWidth: 1
            },
            series: [{
                type: 'scatter',
                id: 'Measurements',
                name: 'Measurements',
                animation: settings.animation,
                data: dataObj.xSeries,
                yAxis: 0,
                color: chartSeriesColors.ProbabilityPlot.mainSeries,
                marker: {
                    enabled: true,
                    radius: 2
                },
                visible: true,
                showInLegend: false
            },
            {
                type: 'line',
                animation: settings.animation,
                data: dataObj.slope,
                yAxis: 0,
                color: chartSeriesColors.ProbabilityPlot.slope,
                marker: {
                    enabled: true,
                    radius: 0
                },
                visible: true,
                showInLegend: false,
                enableMouseTracking: false
                },
                {
                    type: 'line',
                    animation: settings.animation,
                    data: dataObj.probabilityDistributionLine,
                    yAxis: 0,
                    color: chartSeriesColors.ProbabilityPlot.slope,
                    marker: {
                        enabled: true,
                        radius: 0
                    },
                    visible: true,
                    showInLegend: false,
                    enableMouseTracking: false
                }
            ]
        };
        return optionValues;
    }
    function createxPlotLineObject(value, color, lableTxt, xLabel) {
        if (!lableTxt)
            lableTxt = '';

        var xAxis = -35;
        switch (lableTxt.length) {
            case 1:
                xAxis = -15;
                break;
            case 2: 
                xAxis = -18;
                break;
            case 3:
                xAxis = -23;
                break;
            case 4:
                xAxis = -28;
                break;
            case 5:            
                xAxis = -32;
                break;
            case 6:
                xAxis = -37;
                break;
            case 7:
                xAxis = -42;
                break;
            case 8:
                xAxis = -47;
                break;
            default:
                xAxis = -135;
                break;
        }

        var label = {
            'text': lableTxt,
            'align': 'left',
            'x': xAxis,
            'y': 0,
            'style': {
                'fontSize': /*settings.fontSize.labels ||*/ axiesStyle.fontSize,
                'color': 'black'                
            }
        };
        return {
            'id': lableTxt,
            'value': value,
            'width': 2,
            'color': color,
            'x': 0,
            'y': 0,
            'label': label
        };
    }
    function createyPlotLineObject(value, color, text, yLabel, dashStyle, rotation, zIndex) {
        var label;
        label = {
            'text': text,
            'rotation': rotation || 0,
            'align': 'center',
            'x': 0,
            'y': yLabel,
            'style': {
                'fontSize': settings.fontSize.labels || axiesStyle.fontSize,
                'color': color
            }
        };
        return {
            'id': text,
            'value': value,
            'width': 2,
            'color': color,
            'label': label,
            'zIndex': zIndex || 4,
            dashStyle: dashStyle
        };
    }
    function PrepareData() {
        var rowdata = [],
            xPlotLines = [],
            yPlotLines = [],
            xSeries = [],
            probabilityDistributionLine = [],
            xcat = [],
            currentLowerToleranceLimitAbs,
            currentUpperToleranceLimitAbs,
            p0_13,
            p99,
            xMaxValue,
            xMinValue,
            confidenceInterval,
            confidenceIntervalString,
            resultLineX1,
            resultLineY1,
            resultLineX2,
            resultLineY2;
        if (settings.data && settings.data.result && settings.data.result.probabilityPlot && settings.data.result.probabilityPlot.yAxis) {
            $.each(settings.data.result.probabilityPlot.yAxis, function (i, item) {
                var value = roundFloat(item.value,null,true); //  parseFloat(item.value.toFixed(settings.decimalPlaces));
                var text = (item.name || '0') + ' %';
                xPlotLines.push(createxPlotLineObject(value, siemensColors.PL_BLACK_22, text, undefined));
                rowdata.push(value);
            });
        }
        if (settings.data && settings.data.result && settings.data.result.probabilityPlot && settings.data.result.probabilityPlot.measurements) {
            $.each(settings.data.result.probabilityPlot.measurements, function (index, item) {
                var yValue = roundFloat(item.yCoordinate, null, true);
                var value = roundFloat(item.value, null, true);
                var xValue = roundFloat(item.xCoordinate, null, true);
                xSeries.push([value, yValue]);
                xcat.push(xValue);
            });
        }
        if (settings.data && settings.data.result && settings.data.result.probabilityPlot && settings.data.result.probabilityPlot.probabilityDistributionLine) {
            $.each(settings.data.result.probabilityPlot.probabilityDistributionLine, function (index, item) {
                var yValue = roundFloat(item.yValue, null, true);
                var xValue = roundFloat(item.xValue, null, true);
                probabilityDistributionLine.push([xValue, yValue]);
            });
        }
        if (settings.data && settings.data.result && settings.data.result.probabilityPlot) {
            currentLowerToleranceLimitAbs = roundFloat(settings.data.result.probabilityPlot.currentLowerToleranceLimitAbs, null, true);
            currentUpperToleranceLimitAbs = roundFloat(settings.data.result.probabilityPlot.currentUpperToleranceLimitAbs, null, true);
            p0_13 = roundFloat(settings.data.result.probabilityPlot.p0_13, null, true);
            p99 = roundFloat(settings.data.result.probabilityPlot.p99, null, true);
            confidenceInterval = roundFloat(settings.data.specifications.confidenceInterval, null, true);
            confidenceIntervalString = confidenceInterval == null ? '' : confidenceInterval.toString() + getLocalizedText('HIST_Confidance_Interval', settings.locale);
            if (settings.data.result.probabilityPlot.showProbabilityPlotTolerances) {
                xMaxValue = settings.data.result.probabilityPlot.xMaxValue;
                xMinValue = settings.data.result.probabilityPlot.xMinValue;
                resultLineX1 = roundFloat(settings.data.result.probabilityPlot.resultLineX1, null, true);
                resultLineY1 = roundFloat(settings.data.result.probabilityPlot.resultLineY1, null, true);
                resultLineX2 = roundFloat(settings.data.result.probabilityPlot.resultLineX2, null, true);
                resultLineY2 = roundFloat(settings.data.result.probabilityPlot.resultLineY2, null, true);

                yPlotLines.push(createyPlotLineObject(currentLowerToleranceLimitAbs, chartSeriesColors.ProbabilityPlot.ToleranceLimit, getLocalizedText('Hist_currentLowerToleranceLimitAbs', settings.locale), -11, 'Solid'));
                yPlotLines.push(createyPlotLineObject(currentUpperToleranceLimitAbs, chartSeriesColors.ProbabilityPlot.ToleranceLimit, getLocalizedText('Hist_currentUpperToleranceLimitAbs', settings.locale), -11, 'Solid'));
            } else {
                xMaxValue = settings.data.result.probabilityPlot.xMaxValueWithoutTolerances;
                xMinValue = settings.data.result.probabilityPlot.xMinValueWithoutTolerances;
                resultLineX1 = roundFloat(settings.data.result.probabilityPlot.resultLineX1WithoutTolerances, null, true);
                resultLineY1 = roundFloat(settings.data.result.probabilityPlot.resultLineY1WithoutTolerances, null, true);
                resultLineX2 = roundFloat(settings.data.result.probabilityPlot.resultLineX2WithoutTolerances, null, true);
                resultLineY2 = roundFloat(settings.data.result.probabilityPlot.resultLineY2WithoutTolerances, null, true);
            }

            yPlotLines.push(createyPlotLineObject(p0_13, chartSeriesColors.ProbabilityPlot.StandardDeviation, '-' + confidenceIntervalString, -1, 'Solid'));
            yPlotLines.push(createyPlotLineObject(p99, chartSeriesColors.ProbabilityPlot.StandardDeviation, '+' + confidenceIntervalString, -1, 'Solid'));
            xPlotLines.push(createxPlotLineObject(100, siemensColors.PL_BLACK_22));
        }

        var plotLines = [];
        if (settings.data.result.probabilityPlot.showProbabilityPlotTolerances) {
            plotLines = [currentLowerToleranceLimitAbs, currentUpperToleranceLimitAbs, p0_13, p99, xMaxValue, xMinValue];
        }
        else {
            plotLines = [p0_13, p99, xMaxValue, xMinValue];
        }

        plotLines=plotLines.filter(function (el) { return el != null; });
        var maxPlotLinePoint = Math.max.apply(null, plotLines);
        var minPlotLinePoint = Math.min.apply(null, plotLines);
        var dataObj =
        {
            xPlotLines: xPlotLines,
            seriesdata: rowdata,
            xSeries: xSeries,
            xcat: xcat,
            xMinValue: minPlotLinePoint,
            xMaxValue: maxPlotLinePoint,
            slope: [[resultLineX1, resultLineY1], [resultLineX2, resultLineY2]],
            yPlotLines: yPlotLines,
            probabilityDistributionLine: probabilityDistributionLine
        };
        return dataObj;
    }
    function displayTooltip(/*Pnet tooltip*/) {
        var key = this.key;
        if (settings.data && settings.data.result.probabilityPlot && settings.data.result.probabilityPlot.measurements) {
            var msg = "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:600'>" + this.series.name + ": </span><span style='font-size:9pt;font-weight:400'>" + roundFloat(this.y, settings.decimalPlaces, false) + "</span></div>";
            var MeasureObj = settings.data.result.probabilityPlot.measurements.filter(function (obj) {
                return obj.value === key
            });
            if (MeasureObj[0]) {
                var msg = "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:600'>" + this.series.name + ": </span><span style='font-size:9pt;font-weight:400'>" + roundFloat(MeasureObj[0].xCoordinate, settings.decimalPlaces, false) + "</span></div>";
            }
        }
        return msg;
    }
    function setDefaults() {
        if (typeof options === 'object') {
            siemensTooltip.formatter = displayTooltip;
            if (!options.locale || !Highcharts.uiLocale[options.locale]) {
                console.error('Locale ' + options.locale + ' not found, default English locales will be used');
                options.locale = 'en';
            }

            if (typeof options.yAxisUnit !== 'object') options.yAxisUnit = getDefaultyAxisUnitObj();
            else options.yAxisUnit = $.extend(getDefaultyAxisUnitObj(), options.yAxisUnit);


            if (options.fontSize && options.fontSize.labels) {
                axiesTitleStyle.fontSize = options.fontSize.labels;
            }
            if (options.fontSize && options.fontSize.title) {
                titleStyle.fontSize = options.fontSize.title;
            }
            if (options.titleSettings) {
                options.titleSettings.titleStyle = $.extend({}, titleStyle);
                options.titleSettings.titleStyle.transform = 'translate(' + options.titleSettings.x + 'px, ' + options.titleSettings.y + 'px) rotate(' + options.titleSettings.rotate + 'deg)'
            }
        }
    }
    function prepareYAxis(dataObj) {
        var unit = settings.yAxisUnit && isNullOrUndefined(settings.yAxisUnit.text) ? '' : settings.yAxisUnit.text;
        var yAxis =
            [
                {
                    lineColor: siemensColors.PL_BLACK_22,
                    lineWidth: 2,
                    tickAmount: 2,
                    labels: {
                        enabled: false,
                        style: axiesStyle
                    },
                    endOnTick: true,
                    gridLineWidth: 0,
                    minorGridLineWidth: 0,
                    plotLines: dataObj.xPlotLines,
                    title: {
                        text: settings.yAxisTitle || getLocalizedText("pNet_yAxisTitle", options.locale),
                        margin: 60,
                        style: axiesTitleStyle
                    },
                    margin: 15,
                    max: 99
                },
                {// to show units of chart
                    opposite: false,
                    title: {
                        reserveSpace: false,
                        text: unit,
                        align: 'high',
                        rotation: 0,
                        x: settings.yAxisUnit.x,
                        y: settings.yAxisUnit.y,
                        style: {
                            fontSize: settings.yAxisUnit.fontSize,
                            color: siemensColors.PLBlack4
                        }
                    }
                }
            ];

        return yAxis;
    }
    function getDefaultyAxisUnitObj() {
        var obj = {
            text: '',
            x: 60,
            y: -6,
            fontSize: undefined
        };
        return obj;
    }
    function toggleToleranceLineVisibility() {
        // $('#pnet').probabilityPlot('changeToleranceLinesVisibility');
        var chart = $this.data('chartApi');
        settings.data.result.probabilityPlot.showProbabilityPlotTolerances = !settings.data.result.probabilityPlot.showProbabilityPlotTolerances;

        var containerId = $this.attr('id');
        var chartOption = createChartOptions(settings);
        var _chart = Highcharts.chart(containerId, chartOption);
        $this.data('chartApi', _chart);
        $this.data('settings', settings);
    }
    function renderCorrelationCoefficientText() {
        if (!settings.correlationCoefficient.active)
            return;
        var labelText = settings.correlationCoefficient.labelText || getLocalizedText('pNet_correlationCoefficient', options.locale);
        var coeff = roundFloat(settings.data.result.probabilityPlot.correlationCoefficient, settings.decimalPlaces, true) || '';

        if (settings && settings.correlationCoefficient && settings.correlationCoefficient.position) {
            var xPos = settings.correlationCoefficient.position.x;
        } else {
            var xPos = 185;
        }
        if (settings && settings.correlationCoefficient && settings.correlationCoefficient.position) {
            var yPos = settings.correlationCoefficient.position.y;
        } else {
            var yPos = 125;
        }
        var fontSize = settings.correlationCoefficient.fontSize || '13px';
        var fontColor = settings.correlationCoefficient.color || '#666666' ;
        this.renderer.text(labelText + ': ' + coeff, xPos, yPos).css({
            fontSize: fontSize,
            color: fontColor
        }).add();
        
    }
};
/**
 * @description Provide methods for drawing control chart .
 *
 * @namespace AttributiveControlChart
* */
/**
 * @description A new Attributive Control Chart can be drawn using this method.
 * @memberof AttributiveControlChart
 * @function attrControlChart
 * @requires highstock.js
 * @requires jQuery.js
 * @tutorial CreateAttrControlCharts
 * @param {AttributiveControlChart.options} options - The chart options parameter

 * @example
 * $('#CchartC').attrControlChart({
 *                   locale: 'en',
 *                   data: data,
 *                   chartTitle: 'Attributive Control Chart',
 *                   xAxisTitle: 'Custom Sort Number',
 *                   decimalPlaces: 3,
 *                   yAxisTitle: 'Number of Def',
 *                   seriesName: 'xbp',
 *                   height: '60%',
 *                  xAxisLabel: 'customInfo',
 *               });
 */

/**
 * @memberof AttributiveControlChart
 * @typedef {object} options
 * @property {string}  [locale=en]  User locale to display label and violations in a specific language
 * @property {AttributiveControlChart.data} data  JSON Array with subgroup in a defined format {@link AttributiveControlChart.data}
 * @property {boolean}  [xAxisCustomInfoLabel=false] - If true, the default subgroup number label for xAxis will be overridden by the property of customInfo object, where xAxisLabel is true. For more information  please refer {@link AttributiveControlChart.customInfo}
 * @property {string} chartTitle - The Chart title
 * @property {number}  [decimalPlaces=4] - The data of chart will be round based on number of decimal places.
 * @property {number}  [height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number}  [width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {number} yAxisMinValue- a number specifies the min Value of the yAxis
 * @property {number} xAxisMinValue- a number specifies the max Value of the xAxis
 * @property {boolean} [showDefectCollectionCard=false] if true, and the response Object has a defect catalog, a Defect collection Table will be shown at the top of the chart
 * @property {string} [xAxisTitle='Sort number'] - The label for x-Axis.
 * @property {string} [yAxisTitle='Defective Parts|Non Conformance Rate'] - The label for y-Axis. 
 * @property {boolean}  [animation=false] -Enable or disable the initial animation
 * @property {AttributiveControlChart.legendSettings} legendSettings  Settings for position, layout and visibility of the chart legend
 * @property {AttributiveControlChart.titleSettings} [options.titleSettings]  - Provides options to position & rotate the chart title
 * @property {AttributiveControlChart.navigator} [options.navigator]  - The navigator object is to define start and end of navigator range as well as the number of selected values
 * @property {AttributiveControlChart.fontSize} [fontSize]-Option to set Fontsize of label & title.
 */


/**
 * @description The titleSettings object is to determine the position and rotation of the chart title
 * @typedef {object} titleSettings
 * @memberof AttributiveControlChart
 * @property {string} [align = undefined] - The horizontal alignment of the title. Can be one of "left", "center" and "right".
 * @property {number} [rotate = undefined] - The rotation of the text in degrees. 0 is horizontal, 270 is vertical reading from bottom to top.
 * @property {number} [x = undefind] - The x position of the title relative to the alignment
 * @property {number} [y = undefind] - The y position of the title relative to the alignment
 * @example
 *titleSettings:{
 *  align: 'left',
 *  rotate: '90',
 *  x: '35',
 *  y:'207'
 *},
 */

/**
 * @description The navigator object is to define start and end of navigator range as well as the number of selected values
 * @typedef {object} navigator
 * @memberof AttributiveControlChart
 * @property {number} [start = undefined] - first value to be selected in the navigator when defined alone then the last n values would be selected.
 * @property {number} [end = undefined] - last value to be selected in the navigator when defined alone then the first n values would be selected
 * @property {number} [selected = 25] - the number of selected value in the navigator, if start and end are set this value has no effect.
 * @example
 * navigator: {
 *               start: 0,
 *              end: 5,
 *               selected: 5
 *           },
 */

/**
 * @description the values to set the legend to the prefered position or to hide it totaly
 * @typedef {object} legendSettings
 * @memberof AttributiveControlChart
 * @property {boolean} [enabled = true] - show or hide the legend.
 * @property {string} [align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */
 
/**
 * @description The series data format for Attributive Control Chart.
 * @typedef {Object} data
 * @memberof AttributiveControlChart
 * @example
*{
*	"subgroups": [
*		{
*			"subgroupNumber": 1,
*			"statuses": null,
*			"numberOfDefects": 0,
*			"nonConformanceRate": 0.0,
*			"subgroupSize": 20,
*			"upperControlLimitAbs": 5.690521271225974,
*			"lowerControlLimitAbs": 0.0,
*			"processMeanValue": 1.736842105263158,
*			"defects": null,
*			"numberOfMajorDefects": null,
*			"numberOfCriticalDefects": null,
*			"numberOfMinorDefects": null
*		},
*		{
*			"subgroupNumber": 6,
*			"statuses": [
*				{
*					"category": "ucL_1",
*					"viewedPoints": null,
*					"points": null,
*					"lowerPercentage": null,
*					"upperPercentage": null
*				},
*				{
*					"category": "processViolation_1",
*					"viewedPoints": null,
*					"points": null,
*					"lowerPercentage": null,
*					"upperPercentage": null
*				},
*				{
*					"category": "aboveZone",
*					"viewedPoints": null,
*					"points": null,
*					"lowerPercentage": null,
*					"upperPercentage": null
*				}
*			],
*			"numberOfDefects": 8,
*			"nonConformanceRate": 0.4,
*			"subgroupSize": 20,
*			"upperControlLimitAbs": 5.690521271225974,
*			"lowerControlLimitAbs": 0.0,
*			"processMeanValue": 1.736842105263158,
*			"defects": null,
*			"numberOfMajorDefects": null,
*			"numberOfCriticalDefects": null,
*			"numberOfMinorDefects": null
*		}
*	],
*	"result": {
*		"calculatedXbb": 1.736842105263158
*	},
*	"specifications": {
*		"controlChartType": "c"
*	}
*}
*/

/**
 * @description An array of objects with following properties can be injected against each subgroup.
 * @typedef {object} customInfo
 * @memberof AttributiveControlChart
 * @property {string} label - The label to be display in tool-tip.
 * @property {string} value - The value to be used to show in tool-tip and for xAxis labels.
 * @property {boolean} showInTooltip - if true, the label and value will also be shown in tool-tip for a particular subgroup as 'label: value'
 * @property {boolean} xAxisLabel - If true then value of will be shown on xAxis labels instead of default labels. e.g. subgroup number
 * @example
 * <caption>A customInfo can be injected like this, if a data is defined in the given {@link AttributiveControlChart.DataFormat}</caption>

    data.subgroups[0].customInfo = [
        {
            label: 'Charge Number',
            value: 'CH001',
            showInTooltip: true,
            xAxisLabel: false
        },
        {
            label: 'Date',
            value: '05-01-2020',
            showInTooltip: true,
            xAxisLabel: true
        }
    ];
  // Note: If against a subgroup more than one objects have 'xAxisLabel' true then the value by default
  // of the first object will be taken. e.g. If in the given example both objects have xAxisLabel true
  // then in chart the value of first object on xAxis label (for subgroup 0) will be displayed.
  // Which is in this case 'CH001'
 */
/**
 * @typedef {object} fontSize
 * @memberof AttributiveControlChart
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */

$.fn.attrControlChart = function (options) {
    var settings = {};    
    if (options && typeof options === 'object') {
        setDefault();
        settings = $.extend({
            locale: 'en',
            data: {}, // JSON Array with subgroup
            chartTitle: '',
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            yAxisMinValue: undefined,
            yAxisMaxValue: undefined,
            navigator: {
                start: undefined,
                end: undefined,
                selected: 25
            },
            xAxisCustomInfoLabel: false,
            showDefectCollectionCard: false,
            xAxisTitle: getLocalizedText('SVC_xAxisTitle', settings.locale),
            yAxisTitle: getSeriesTitle(),
            animation: false,
            titleSettings: undefined,
            legendSettings: {
                enabled: true,
                align: 'right',
                verticalAlign: 'top',
                layout: 'vertical',
                x: 0,
                y: 100
            }
        }, options);
    } else if (typeof options === 'string') { // Method call
        settings = $(this).data('settings') || {};  // restore the settings object from prev settings
    }  else if (!options)
        throw 'Not valid options or parameter to the "attrControlChart" widget passed';
    var $this = this,
        _args = arguments,
        chart = {
            init: function () {
                var containerId = $this.attr('id');
                var _chart = Highcharts.stockChart(containerId, chartOption);
                drawDefectTable(containerId);

                $this.data('chartApi', _chart);
                // $this.data('inputData', input); // to get the series data on method call
                $this.data('settings', settings);
            },
            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof AttributiveControlChart
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered.
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#CchartC').attrControlChart('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64',
            *                                           width : 1800,
            *                                           height: 600,
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        },
        attrChartTYpe = parseAttrChartType(settings.data.specifications.controlChartType),
        chartOption = createChartOptions(settings);
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function createChartOptions(settings) {
        if (!settings.data || !settings.data.subgroups)
            throw 'Invalid data format exception: Series data is not in defined format';
        var series = {
            NonConformanceRate: [],
            DefectiveParts: [],
            UCLAbs: [],
            processMeanValue: [],
            flagSeries: [],
            customInfo: [],
            LCLAbs: [],
            sortNumbers: [],
            localMeanValue: []            
        },
            siemens_tooltip = $.extend({}, siemensTooltip);

        settings.inputs = series;
        prepareData(settings.data, series);
        var optionValues = {
            chart: {
                type: 'line',
                animation: settings.animation,
                height: settings.height,
                width: settings.width,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                }
            },
            credits: {
                enabled: false
            },
            plotOptions: {
                series: {
                    animation: settings.animation,
                    states: {
                        inactive: {
                            opacity: 0.8
                        }
                    }
                }
            },
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            navigator: {
                baseSeries: 1,
                enabled: true,
                xAxis: {
                    labels: {
                        formatter: function () {
                            return this.value;
                        }
                    }
                },
                series: {
                    lineColor: chartSeriesColors.SingleValueChart.Measurement
                }
            },
            rangeSelector: {
                enabled: false
                // selected: 5  // date, month year etc
            },
            title: {
                text: settings.chartTitle,
                align: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.align)) ? 'center' : settings.titleSettings.align,
                style: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.titleStyle)) ? titleStyle : settings.titleSettings.titleStyle,
            },
            tooltip: siemens_tooltip, // siemensTooltip,
            xAxis: {
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                gridLineColor: siemensColors.PL_BLACK_22,
                title: {
                    text: settings.xAxisTitle, //  getLocalizedText('SVC_xAxisTitle', settings.locale), 
                    style: axiesTitleStyle
                },
                labels: {
                    rotation: -90,
                    formatter: function () {
                        return series.sortNumbers[this.value];
                    },
                    style: axiesStyle
                },
                min: settings.navigator.start,
                max: settings.navigator.end,
                range: settings.navigator.selected, // select last 25 samples
                categories: series.sortNumbers,
                tickInterval: 1,
                tickPixelInterval: 15,
                gridLineWidth: 1
            },
            yAxis: [{
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                min: settings.yAxisMinValue,
                max: settings.yAxisMaxValue,
                opposite: false,
                labels: {
                    formatter: function () {
                        return roundFloat(this.value, settings.decimalPlaces); // parseFloat(this.value).toFixed(settings.decimalPlaces);
                    },
                    style: axiesStyle
                },
                title: {
                    text: settings.yAxisTitle, //  getSeriesTitle(),
                    style: axiesTitleStyle,
                    margin: 60
                }
            }
            ],
            series: prepareSeries(series)
        };
        validateLegendPositioning(optionValues.legend);

        return optionValues;
    }

    function prepareData(data, series) {
        $.each(data.subgroups, function (index, item) {
            var customInfo = item.customInfo || [],
                nonConformanceRate,
                numberOfDefects,
                subgroupNumber,
                upperControlLimitAbs,
                lowerControlLimitAbs,
                processMeanValue,
                calculatedXbb;

            // fill the variable with data
            nonConformanceRate = roundFloat(item.nonConformanceRate);
            numberOfDefects = roundFloat(item.numberOfDefects);
            subgroupNumber = roundFloat(item.subgroupNumber);
            upperControlLimitAbs = roundFloat(item.upperControlLimitAbs);
            lowerControlLimitAbs = roundFloat(item.lowerControlLimitAbs);
            processMeanValue = roundFloat(item.processMeanValue);
            if (data.result)
                calculatedXbb = roundFloat(data.result.calculatedXbb);
            // fill xAxis Labels & check for custom Info
            if (settings.xAxisCustomInfoLabel === true) {    // for xAxis
                if (customInfo.length > 0)
                    series.sortNumbers.push(getCustomxAxisLabel(customInfo));
                else
                    series.sortNumbers.push(null); // show empty, if no corresponding customInfo is given
            } else
                series.sortNumbers.push(subgroupNumber);
            //Fill series
            series.customInfo.push(customInfo);
            series.UCLAbs.push(upperControlLimitAbs);
            series.LCLAbs.push(lowerControlLimitAbs);
            series.processMeanValue.push(processMeanValue);
            series.localMeanValue.push(calculatedXbb);

            var countChartViolations = data.specifications.calculateCUCountChart;

            var statuses = item.statuses;
            var newstatuses = [];
            if (statuses && statuses.length > 0) {
                for (var i = 0; i < statuses.length; i++)
                    newstatuses.push(strToLowerCase(statuses[i].category)); //TODO: should be case insensitive

                if (newstatuses.indexOf("attachment") > -1) {
                    var YValue;
                    switch (attrChartTYpe) {
                        case attrChartTypes.C:
                        case attrChartTypes.np:
                            YValue = numberOfDefects;
                            break;
                        case attrChartTypes.p:
                        case attrChartTypes.U:
                            YValue = nonConformanceRate;
                            break;
                    }
                    series.flagSeries.push({
                        //y: series1Val, 
                        y: YValue, 
                        x: index,
                        title: svgRepository.Attachment,
                        text: getLocalizedText('Attachment', settings.locale)
                    });
                }
                if (newstatuses.indexOf("processviolation_1") > -1 && countChartViolations === false) {
                    series.NonConformanceRate.push({
                        marker: {
                            fillColor: chartSeriesColors.AttrControlChart.Marker,
                            lineWidth: 3,
                            lineColor: chartSeriesColors.ControlChart.Marker,
                            symbol: 'triangle'
                        },
                        name: 'violation' + index,
                        y: nonConformanceRate,
                        vType: newstatuses
                    });


                    series.DefectiveParts.push({
                        marker: {
                            fillColor: chartSeriesColors.AttrControlChart.Marker,
                            lineWidth: 3,
                            lineColor: chartSeriesColors.ControlChart.Marker,
                            symbol: 'triangle'
                        },
                        name: 'violation' + index,
                        y: numberOfDefects,
                        vType: newstatuses
                    });
                } else {
                    series.NonConformanceRate.push(nonConformanceRate);
                    series.DefectiveParts.push(numberOfDefects);
                }

                if (newstatuses.indexOf("eliminated") > -1) {
                    switch (attrChartTYpe) {
                        case attrChartTypes.C:
                        case attrChartTypes.np:
                            series.flagSeries.push({
                                y: numberOfDefects,
                                x: index,
                                title: 'X',
                                text: getLocalizedText('eliminated', settings.locale)
                            });
                            break;
                        case attrChartTypes.p:
                        case attrChartTypes.U:
                            series.flagSeries.push({
                                y: nonConformanceRate,
                                x: index,
                                title: 'X',
                                text: getLocalizedText('eliminated', settings.locale)
                            });
                            break;
                    }
                }
            } else {
                series.NonConformanceRate.push(nonConformanceRate);
                series.DefectiveParts.push(numberOfDefects);
            }
        });
    }

    function prepareSeries(series) {
        var mainSeriesdata,
            mainSeriesName;
        switch (attrChartTYpe) {
            case attrChartTypes.C:
            case attrChartTypes.np:
                mainSeriesName = getLocalizedText('DefectiveParts', settings.locale);
                mainSeriesdata = series.DefectiveParts;
                break;
            case attrChartTypes.p:
            case attrChartTypes.U:
                mainSeriesName = getLocalizedText('NonConformanceRate', settings.locale);
                mainSeriesdata = series.NonConformanceRate;
        }
        var ser = [
            {
                id: 'UCLAbs',
                animation: settings.animation,
                name: getLocalizedText('UCL_1', settings.locale),
                data: series.UCLAbs,
                yAxis: 0,
                color: chartSeriesColors.AttrControlChart.ControlLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 2,
                legendIndex: 1,
                showInLegend: shouldDisplaySeries(series.UCLAbs)
            },
            {
                id: 'main',
                name: mainSeriesName,
                data: mainSeriesdata,
                animation: settings.animation,
                yAxis: 0,
                color: chartSeriesColors.AttrControlChart.main,
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                zIndex: 999,
                marker: {
                    enabled: true,
                    radius: 4
                },
                events: {
                    click: function (e) {
                        if (!$('#defectTableContainer').length)
                            return;

                        var subgroupNumber = 0;
                        if (e && e.point)
                            subgroupNumber = e.point.index + 1;

                        var offset = $('#' + subgroupNumber).position().left;
                        offset = offset > 100 ? offset - 100 : offset;
                        $('#defectTableContainer').animate({
                            scrollLeft: offset - 100
                        }, 'slow');
                        $('.subGroup').css('border-style', 'none');
                        $('.' + subgroupNumber).css('border-left', '1px solid red');
                        $('.' + subgroupNumber).css('border-right', '1px solid red');
                        $('.' + subgroupNumber).css('border-left', '1px solid red');
                        $('#' + subgroupNumber).css('border-right', '1px solid red');
                        $('#' + subgroupNumber).css('border-left', '1px solid red');
                        $('#' + subgroupNumber).css('border-top', '1px solid red');
                        $('.' + subgroupNumber + '.last').css('border-bottom', '1px solid red');
                    }
                },
                lineWidth: 2,
                legendIndex: 2,
                zoneAxis: 'x',
                zones: prepareColoredZone()
            },
            {
                id: 'processMeanValue',
                animation: settings.animation,
                name: getLocalizedText('processMeanValue', settings.locale),
                data: series.processMeanValue,
                yAxis: 0,
                color: chartSeriesColors.AttrControlChart.ProcessMeanValue,
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 2,
                legendIndex: 3,
                showInLegend: shouldDisplaySeries(series.processMeanValue)
            },
            {
                id: 'localMeanValue',
                animation: settings.animation,
                name: getLocalizedText('localMeanValue', settings.locale),
                data: series.localMeanValue,
                yAxis: 0,
                color: chartSeriesColors.AttrControlChart.localMeanValue,
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 2,
                legendIndex: 4,
                showInLegend: shouldDisplaySeries(series.localMeanValue)
            },
            {
                id: 'LCLAbs',
                animation: settings.animation,
                name: getLocalizedText('LCL_1', settings.locale),
                data: series.LCLAbs,
                yAxis: 0,
                color: chartSeriesColors.AttrControlChart.ControlLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false
                },
                lineWidth: 2,
                legendIndex: 5,
                showInLegend: shouldDisplaySeries(series.LCLAbs)
            },
            {
                id: seriesIds.AtributiveControlChart.atributiveflagSeries,
                type: 'flags',
                animation: settings.animation,
                data: series.flagSeries,
                onSeries: mainSeriesName,
                showInLegend: false,
                useHTML: true,
                dataLabels: {
                    useHTML: true,
                },
                lineWidth: 1,
                lineColor: '#005F87',
                stackDistance: 10
            }
        ];
        return ser;
    }

    function displayTooltip(/*tooltip-Atr*/) {
        var msg = [],
            customInfo = settings.inputs.customInfo || [];

        msg.push('<b>' + this.series.name + ' : ' + roundFloat(this.y, settings.decimalPlaces) + '</b>');

        if (customInfo.length > this.point.index && customInfo[this.point.index].length > 0) {
            msg.push('<hr style="margin-top: 7px; padding: 0; margin-bottom: -10px; "/>');
            msg.push(getCustomTooltip(customInfo[this.point.index]));
        }

        if (this.key.toString().startsWith('violation')) {
            if (this.point && this.point.vType && this.point.vType.length > 0 && typeof this.point.vType === 'object') {
                var arr = this.point.vType;
                for (var v in arr) {
                    if (arr[v] && arr[v].toLowerCase() !== 'processviolation_2' && arr[v].toLowerCase() !== 'processviolation_1') {
                        msg.push(getLocalizedText(arr[v], settings.locale));
                    }
                }
            }
        }
        if (this.point.options.text) // for flags type series, only it should be shown
            return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + this.point.options.text + "</span></div>";
        return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + msg.join('<br>') + "</span></div>";
    }

    function setDefault() {
        if (typeof options === 'object') {
            siemensTooltip.formatter = displayTooltip;
            if (options && typeof options === 'object') {
                if (!options.locale || !Highcharts.uiLocale[options.locale]) {
                    console.error('Warning: Locale "' + options.locale + '" not found, default English locales will be used');
                    options.locale = 'en';
                }
            }

            if (options.fontSize && options.fontSize.labels) {
                axiesTitleStyle.fontSize = options.fontSize.labels;
            }
            if (options.fontSize && options.fontSize.title) {
                titleStyle.fontSize = options.fontSize.title;
            }

            if (options.titleSettings) {
                options.titleSettings.titleStyle = $.extend({}, titleStyle);
                options.titleSettings.titleStyle.transform = 'translate(' + options.titleSettings.x + 'px, ' + options.titleSettings.y + 'px) rotate(' + options.titleSettings.rotate + 'deg)'
            }
        }
    }

    function getCustomxAxisLabel(customInfo) {
        if (customInfo) {
            var xAxisObj = customInfo.filter(function (k) { return k.xAxisLabel === true; });
            if (xAxisObj && xAxisObj.length > 0 && xAxisObj[0].value)
                return xAxisObj[0].value;
        }
        return '';
    }

    function getCustomTooltip(customInfo) {
        var customTooltip = [];

        if (customInfo) {
            var xAxisObj = customInfo.filter(function (k) { return k.showInTooltip === true; });
            if (xAxisObj) {
                xAxisObj.forEach(function (o) {
                    customTooltip.push(o.label + ' : ' + o.value);
                });
            }
        }
        return customTooltip.join('<br>');
    }
    function getSeriesTitle() {
        switch (attrChartTYpe) {
            case attrChartTypes.C:
            case attrChartTypes.np:
                return getLocalizedText('DefectiveParts', settings.locale);
            case attrChartTypes.p:
            case attrChartTypes.U:
                return getLocalizedText('NonConformanceRate', settings.locale);
            default:
                return '';
        }
    }
    function drawDefectTable(containerId) {
        if (!settings.showDefectCollectionCard)
            return;
        if ($('#defectTableContainer').length) {
            $('#defectTableContainer').remove();
        }
        if (!settings.data.subgroups || !settings.data.result || !settings.data.result.defectPareto)
            return;
        setTimeout(function () {
            var tableWidth = roundFloat($("rect.highcharts-plot-background").attr("width"));
            var tableleftpos = roundFloat($("rect.highcharts-plot-background").attr("x"));
            var groupwidth = 112;

            var defectTableContainer = $("<div />", { id: "defectTableContainer" }),
                defectTableHeader = $("<thead />", { id: "defectTableHeader" }),
                defectTableHeaderRow = $("<tr />", { id: "defectTableHeaderRow" }),
                defectTableBody = $("<tbody />", { id: "defectTableBody" }),
                defectsHeadHtml = $("<th />", { class: "defectThead" });

            var defectTable = $("<Table />", { id: "defectTable" })
                .append(defectTableHeader)
                .append(defectTableHeaderRow).
                append(defectTableBody);

            var htmlArr = [];
            var bodyhtmlArr = [];
            defectTable.css('table-layout', 'fixed');
            var defectTableContainerCss = { 'width': tableWidth + tableleftpos, 'overflow': 'scroll', 'position': 'relative', 'left': '0', 'padding-bottom': '3px' }
            defectTableContainer.css(defectTableContainerCss);
            //defectTableContainer.css("overflow", "scroll");
            //defectTableContainer.css("position", "relative");
            //defectTableContainer.css("left", "0");
            //defectTableContainer.css("padding-bottom", "3px");
            defectTableContainer.append(defectTable);
            defectTableContainer.insertBefore($('#' + containerId));
            var defectsHeadColumnCss = { 'width': 117, 'position': 'sticky', 'left': '0', 'top': 'auto', 'background-color': '#0A7CA4'}
            defectsHeadHtml.css(defectsHeadColumnCss);
            defectsHeadHtml.html('Defects');

            $.each(settings.data.subgroups, function (index, item) {
                htmlArr.push(
                    '<th id=' + roundFloat(item.subgroupNumber) + ' class="subGroup"style="background-color:#0A7CA4 ; width:' + groupwidth + 'px" >' + roundFloat(item.subgroupNumber) + '</th>'
                );
            });
            var lastItem = settings.data.result.defectPareto[settings.data.result.defectPareto.length - 1];
            $.each(settings.data.result.defectPareto, function (index, item) {
                if (item === lastItem) {
                    bodyhtmlArr.push
                        (
                            '<tr><td style="word-break:break-all;position: sticky; left: 0;top:auto;background-color:#0A7CA4" >' + item.defectID + '</td>' + drawSupgroups(item.defectID, groupwidth) + '<tr>'
                        );
                } else
                    bodyhtmlArr.push
                        (
                            '<tr><td style="word-break:break-all;position: sticky; left: 0;top:auto;background-color:#0A7CA4" >' + item.defectID + '</td>' + drawSupgroups(item.defectID, groupwidth) + '<tr>'
                        );
            });

            bodyhtmlArr.push('<tr><td style="word-break:break-all;position: sticky; left: 0;top:auto;background-color:#0A7CA4" >Total</td>' + calcDefectSum() + '<tr>');
            defectTableHeaderRow.append(defectsHeadHtml);
            defectTableHeaderRow.append(htmlArr.join(' '));
            defectTableBody.append(bodyhtmlArr.join(' '));
        }, 1000);
    }
    function drawSupgroups(defectID, groupwidth) {
        var htmlArr = [];
        $.each(settings.data.subgroups, function (index, item) {
            var defect = item.defects.filter(function (obj) {
                return obj.defectID === defectID;
            });
            htmlArr.push(

                '<td class="' + roundFloat(item.subgroupNumber) + ' subGroup" style="color:#000 ; width:' + groupwidth + 'px">' + defect[0].numberOfDefects + '</td>'
            );
        });
        return htmlArr.join(' ');
    }
    function calcDefectSum() {
        var htmlArr = [];
        $.each(settings.data.subgroups, function (index, item) {
            var defects = item.defects;

            var sum = defects.reduce(function (a, b) {
                return a + b['numberOfDefects'];
            }, 0);

            htmlArr.push(

                '<td class="' + roundFloat(item.subgroupNumber) + ' subGroup last">' + sum + '</td>'
            );
        });
        return htmlArr.join(' ');
    }

    function prepareColoredZone() {
        var zone = [];
        var interval;

        //Disply violated samples and values in a different color 
        if (settings.data && settings.data.subgroups) {
            var violatedSubgroupsNumber = [];
            settings.data.subgroups.forEach(function (subgroup) {
                var statuses = subgroup.statuses;
                var newstatuses = [];
                if (statuses && statuses.length > 0) {
                    for (var i = 0; i < statuses.length; i++)
                        newstatuses.push(strToLowerCase(statuses[i].category));
                    if (newstatuses.indexOf("involvedbyother") > -1) {
                        violatedSubgroupsNumber.push(roundFloat(subgroup.subgroupNumber));
                    }
                }
            });

        }
        interval = getSequence(violatedSubgroupsNumber)
        zone = getColoredSeriesZone(interval, siemensColors.violationZone);
        return zone;


    }

    function shouldDisplaySeries(series) {
        var notNullSeries = [];
        if (series && series.filter) {
            notNullSeries = series.filter(function (k) {
                if (k !== null && k !== undefined)
                    return k.toString();
            });
        }
        var nonullableSeries = notNullSeries.length > 0;
        return nonullableSeries;
    }
};
/**
 * @description Provide methods for drawing the defect Pareto chart .
 *
 * @namespace DefectPareto
 */

/**
 * @description A new defect Pareto chart can be drawn using this method.
 * @memberof DefectPareto
 * @function defectPareto
 * @requires highstock.js
 * @requires jQuery.js
 * @param {DefectPareto.options} options - The chart options parameter
 * @example
 * $('#paretoChart').defectPareto({
 *              locale: locale || 'en',
 *              data: data,
 *              decimalPlaces: 4,
 *              height: '550px',
 *              chartTitle: undefined,
 *              xAxisTitle: undefined,
 *              yAxisTitle: undefined
 *          });
 */

/**
 * @description the legend position settings
 * @typedef {object} legendSettings
 * @memberof DefectPareto
 * @property {boolean} [legendSettings.enabled = true] - show or hide the legend.
 * @property {string} [legendSettings.align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [legendSettings.verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [legendSettings.layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [legendSettings.x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [legendSettings.Y = 100] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */

/**
 * @memberof DefectPareto
 * @typedef {object} options
 * @property {string}  [locale=en]  User locale to display label and violations in a specific language
 * @property {DefectPareto.legendSettings} legendSettings  Settings for position, layout and visibility of the chart legend {@link DefectPareto.legendSettings}
 * @property {DefectPareto.data} data  Array of defect in a defined format {@link DefectPareto.data}
 * @property {number}  [height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number}  [width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string} [xAxisTitle='Defect'] - The xAxis Title
 * @property {string} [yAxisTitle='Count'] - The yAxis Title
 * @property {string} chartTitle - The chart Title
 * @property {boolean}  [animation=false] -Enable or disable the initial animation
 * @property {DefectPareto.titleSettings} [options.titleSettings]  - Provides options to position & rotate the chart title
 */

/**
 * @description The titleSettings object is to determine the position and rotation of the chart title
 * @typedef {object} titleSettings
 * @memberof DefectPareto
 * @property {string} [align = undefined] - The horizontal alignment of the title. Can be one of "left", "center" and "right".
 * @property {number} [rotate = undefined] - The rotation of the text in degrees. 0 is horizontal, 270 is vertical reading from bottom to top.
 * @property {number} [x = undefind] - The x position of the title relative to the alignment
 * @property {number} [y = undefind] - The y position of the title relative to the alignment
 * @example
 *titleSettings:{
 *  align: 'left',
 *  rotate: '90',
 *  x: '35',
 *  y:'207'
 *},
 */

/**
 * @description The series data format for Defect Pareto  Chart.
 * @typedef {Object} data
 * @memberof DefectPareto
 * @example
*   [
*	    {
*		    "defectID": "DefDefect5",
*		    "classification": "critical",
*		    "numberOfDefects": 170,
*		    "percentage": 14.492753623188407
*	    },
*	    {
*		    "defectID": "DefDefect4",
*		    "classification": "critical",
*		    "numberOfDefects": 145,
*		    "percentage": 12.361466325660699
*	    }
*   ]
*/
$.fn.defectPareto = function (options) {
    setDefaults();
    if (options && typeof options === 'object') {
        var settings = $.extend({
            locale: 'en',
            data: {},
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            chartTitle: undefined,
            xAxisTitle: getLocalizedText('Defect_Pareto_xAxis', options.locale),
            yAxisTitle: getLocalizedText('Defect_Pareto_yAxis', options.locale),
            titleSettings: undefined,
            legendSettings: {
                enabled: true,
                align: 'right',
                verticalAlign: 'top',
                layout: 'vertical',
                x: 0,
                y: 100
            },
            animation:false
        }, options);
    } else if (!options)
        throw 'Not valid options or parameter to the "chart" widget passed';
    var $this = this,
        _args = arguments,
        chartOption = createChartOptions(settings),
        chart = {
            init: function () {
                var containerId = $this.attr('id');
                //var _chart =
                Highcharts.chart(containerId, chartOption);
            },
            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof DefectPareto
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered.
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#defectPareto').defectPareto('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64',
            *                                           width : 1800,
            *                                           height: 600,
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        };
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function createChartOptions(settings) {
        var input = {
            categories: [],
            defectsCount: []
        },
            siemens_tooltip = $.extend({}, siemensTooltip);

        prepareData(input);
        var optionValues = {
            chart: {
                height: settings.height,
                animation: settings.animation,
                width: settings.width,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                },
                type: 'column'
            },
            title: {
                text: options.chartTitle || getLocalizedText('Pareto_chart_title', options.locale),
                align: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.align)) ? 'center' : settings.titleSettings.align,
                style: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.titleStyle)) ? titleStyle : settings.titleSettings.titleStyle,
            },
            plotOptions: {
                series: {
                    animation: settings.animation,
                }
            },
            credits: {
                enabled: false
            },
            tooltip: siemens_tooltip, // siemensTooltip,
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            yAxis: {
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                labels: {
                    enabled: true,
                    style: axiesStyle,
                    formatter: function () {
                        return this.value + ' %'
                    }
                },
                title: {
                    text: settings.yAxisTitle, //  options.yAxisTitle || getLocalizedText('Defect_Pareto_yAxis', options.locale), 
                    style: axiesTitleStyle
                },
            },
            xAxis: {
                title: {
                    text: settings.xAxisTitle, //  options.xAxisTitle || getLocalizedText('Defect_Pareto_xAxis', options.locale), 
                    style: axiesTitleStyle
                },
                gridLineColor: siemensColors.PL_BLACK_22,
                categories: input.categories,
                labels: {
                    style: axiesStyle
                }
            },
            series: prepareSeries(input)
        };
        validateLegendPositioning(optionValues.legend);

        return optionValues;
    }
    function prepareData(input) {
        $.each(settings.data, function (index, defect) {
            input.categories.push(roundFloat(defect.defectID));
            input.defectsCount.push(roundFloat(defect.percentage));
        });
    }
    function prepareSeries(input) {
        var ser = [
            {
                name: 'Defects',
                data: input.defectsCount,
                color: chartSeriesColors.Pareto.DefectsCount,
                animation: settings.animation,
                states: {
                    hover: {
                        color: chartSeriesColors.Pareto.DefectsCounthover
                    }
                }
            }
        ];
        return ser;
    }
    function displayTooltip(/*tooltip*/) {
        if (settings.data) {
            var msg = "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:600'>" + this.series.name + ": </span><span style='font-size:9pt;font-weight:400'>" + roundFloat(this.y, settings.decimalPlaces) + "</span></div>";
            var key = this.x;
            var defectObj = settings.data.filter(function (obj) {
                return obj.defectID === key;
            });
            if (defectObj[0]) {
                var defectIDcaption = "<span style='font-size: 9pt; font-weight: 600'>" + getLocalizedText('Defect_Pareto_ID_Tooltip_Caption', settings.locale) + ":</span><span style='font-size: 9pt; font-weight: 400'> " + roundFloat(defectObj[0].defectID) + "</span><br>";
                var defectCountCaption = "<span style='font-size: 9pt; font-weight: 600'>" + getLocalizedText('Defect_Pareto_count_Tooltip_Caption', settings.locale) + ":</span><span style='font-size: 9pt; font-weight: 400'> " + roundFloat(defectObj[0].numberOfDefects) + "</span><br>";
                var defectPercentCaption = "<span style='font-size: 9pt; font-weight: 600'>" + getLocalizedText('Defect_Pareto_percent_Tooltip_Caption', settings.locale) + ":</span><span style='font-size: 9pt; font-weight: 400'> " + roundFloat(defectObj[0].percentage, settings.decimalPlaces) + " %</span><br>";
                var defectclass = "<span style='font-size: 9pt; font-weight: 600'>" + getLocalizedText('Defect_Pareto_class_Tooltip_Caption', settings.locale) + ":</span><span style='font-size: 9pt; font-weight: 400'> " + defectObj[0].classification + "</span><br>";
                msg = "<div style='padding:8px 16px'>" + defectIDcaption + defectCountCaption + defectPercentCaption + defectclass + "</div>";
            }
        }
        return msg;
    }
    function setDefaults() {
        if (typeof options === 'object') {
            siemensTooltip.formatter = displayTooltip;
            if (!options.locale || !Highcharts.uiLocale[options.locale]) {
                console.error('Locale ' + options.locale + ' not found, default English locales will be used');
                options.locale = 'en';
            }

            if (options.fontSize && options.fontSize.labels) {
                axiesTitleStyle.fontSize = options.fontSize.labels;
            }
            if (options.fontSize && options.fontSize.title) {
                titleStyle.fontSize = options.fontSize.title;
            }

            if (options.titleSettings) {
                options.titleSettings.titleStyle = $.extend({}, titleStyle);
                options.titleSettings.titleStyle.transform = 'translate(' + options.titleSettings.x + 'px, ' + options.titleSettings.y + 'px) rotate(' + options.titleSettings.rotate + 'deg)'
            }
        }
    }
};
/**
 * @description Provide methods to draw cumulative sum chart .
 *
 * @namespace CumulativeSumChart
 *
 */

/**
 * @description Draw cumulative sum chart with the given parameters.
 * @memberof CumulativeSumChart
 * @function cumulativeSumChart
 * @requires highstock.js
 * @requires jQuery.js
 * @param {CumulativeSumChart.options} options the chart options parameter
 * @example
 * $('#CUSum').cumulativeSumChart({
 *                      locale: locale,
 *                      data: {
 *                              subgroups: [
 *                                            {
 *                                               cuSumPlus: 0.001049999999999985,
 *                                               cuSumMinus: 0
 *                                             }
 *                                          ],
 *                              result : {
 *                                  cuSum : {
 *                                              upperControlLimit: 0.1116,
 *                                              lowerControlLimit: -0.1116,
 *                                          }
 *                                      },
 *                      decimalPlaces: 4,
 *                      height: 550,
 *                      width: 400,
 *                      });
 *
 */

/**
 * @typedef {object} options
 * @memberof CumulativeSumChart
 * @property {string} [options.locale=en]  User locale for tooltip and title etc. It require corresponding localization files to be referenced in html e.g. ui-charts-de.js should be included for german locale.
 * @property {CumulativeSumChart.data} options.data - JSON Array with subgroup
 * @property {string} [options.chartTitle = 'Cumulative Sum Chart'] - The default value will be based on corresponding locale (if referenced).
 * @property {number} [options.decimalPlaces = 4] - Number of decimal places for numbers to be shown in tooltip.
 * @property {number} [options.height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number} [options.width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string} [options.xAxisTitle='Subgroups'] - The label for x-Axis.
 * @property {string} [options.yAxisTitle='CUSUM'] - The label for y-Axis.
 * @property {boolean}  [animation=false] -Enable or disable the initial animation
 * @property {CumulativeSumChart.legendSettings} legendSettings - Settings for position, layout and visibility of the chart legend
 * @property {CumulativeSumChart.navigator} [options.navigator]  - The navigator object is to define start and end of navigator range as well as the number of selected values
 * @property {CumulativeSumChart.fontSize} [fontSize]-Option to set Fontsize of label & title.
*/

/**
 * @description The format of data for cumulative sum chart.
 * @typedef {Array} data - The array of objects.
 * @memberof CumulativeSumChart
 * @property {Array} subgroups - The array of objects in defined format. Please look the format in the in example below.
 * @property {object} result - An object containg additional information about cumulative sum chart.
 * @property {object} result.cuSum - The additional attribute of sumulative sum chart.
 * @example
 * var data = {
 *      subgroups: [
 *              {
 *                  cuSumHigh: 0.001049999999999985
 *                  cuSumLow: 0,
 *              }
 *            ],
 *      result : {
 *          cuSum:{
 *              upperControlLimit: 0.1116,
 *              lowerControlLimit: -0.1116,
 *          }
 *  }
 */
/**
 * @typedef {object} legendSettings
 * @memberof CumulativeSumChart
 * @property {boolean} [legendSettings.enabled = true] - show or hide the legend.
 * @property {string} [legendSettings.align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [legendSettings.verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [legendSettings.layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [legendSettings.x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [legendSettings.Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */

/**
 * @description The navigator object is to define start and end of navigator range as well as the number of selected values
 * @typedef {object} navigator
 * @memberof CumulativeSumChart
 * @property {number} [start = undefined] - first value to be selected in the navigator when defined alone then the last n values would be selected.
 * @property {number} [end = undefined] - last value to be selected in the navigator when defined alone then the first n values would be selected
 * @property {number} [selected = 25] - the number of selected value in the navigator, if start and end are set this value has no effect.
 * @example
 * navigator: {
 *               start: 0,
 *               end: 5,
 *               selected: 5
 *           },
 */

/**
 * @description An array of objects with following properties can be injected against each subgroup.
 * @typedef {object} customInfo
 * @memberof CumulativeSumChart
 * @property {string} label - The label to be display in tooltip.
 * @property {string} value - The value to be used to show in tooltip and for xAxis labels.
 * @property {boolean} showInTooltip - if true, the label and value will also be showin in tooltip for a particular subgrup as 'label: value'
 * @example
 * <caption>A customInfo can be injected like this, if a data is defined in the given {@link CumulativeSumChart.data}</caption>

    data.subgroups[0].customInfo = [
        {
            label: 'Charge Number',
            value: 'CH001',
            showInTooltip: true,
            xAxisLabel: false
        },
        {
            label: 'Date',
            value: '05-01-2020',
            showInTooltip: true,
            xAxisLabel: true
        }
    ];
  // Note: If against a subgroup more than one objects have 'xAxisLabel' true then the value by default
  // of the first object will be taken. e.g. If in the given example both objects have xAxisLabel true
  // then in chart the value of first object on xAxis label (for subgroup 0) will be displayed.
  // Which is in this case 'CH001'
  */
/**
 * @typedef {object} fontSize
 * @memberof CumulativeSumChart
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */

$.fn.cumulativeSumChart = function (options) {
    var settings = {};
    if (options && typeof options === 'object') {
        setDefault();
        settings = $.extend({
            locale: 'en',
            data: {}, // JSON Array with subgroup
            chartTitle: getLocalizedText('CUSum_chart', options.locale),
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            xAxisTitle: getLocalizedText('x_axis_subgroups', options.locale),
            yAxisTitle: 'CUSUM',
            navigator: {
                start: undefined,
                end: undefined,
                selected: 25
            },
            legendSettings: {
                enabled: true,
                // left, center and right
                align: 'right',
                //top, middle or bottom.
                verticalAlign: 'top',
                // horizontal or vertical or proximate
                layout: 'vertical',
                x: 0,
                y: 100
            }
        }, options);
    } else if (!options)
        throw 'Not valid options or parameter to the "chart" widget passed';
    var $this = this,
        _args = arguments,
        chartOption = createChartOptions(settings),
        chart = {
            init: function () {
                var containerId = $this.attr('id');
                Highcharts.stockChart(containerId, chartOption);
            },
            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof CumulativeSumChart
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered.
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#CumulativeSumChart').CumulativeSumChart('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64',
            *                                           width : 1800,
            *                                           height: 600,
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        };

    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function createChartOptions(settings) {
        if (!settings.data || !settings.data.subgroups)
            throw 'Invalid data format exception: Series data is not in defined format';
        var series = { cuSumLow: [], cuSumHigh: [], cuSumUCL: [], cuSumLCL: [], cuSumCL: [], customInfo: [], xAxisLabels: [] },
            yMin = settings.data.result && settings.data.result.cuSum ? Math.min(settings.data.result.cuSum.lowerControlLimit, settings.data.result.cuSum.minCusumMinus) || null : null,
            yMax = settings.data.result && settings.data.result.cuSum ? Math.max(settings.data.result.cuSum.upperControlLimit, settings.data.result.cuSum.maxCUSumPlus) || null : null,
            siemens_tooltip = $.extend({}, siemensTooltip);

        prepareData(series);
        settings.inputs = series;

        var optionValues = {
            chart: {
                type: 'line',
                height: settings.height,
                width: settings.width,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                }
            },
            credits: {
                enabled: false
            },
            plotOptions: {
                series: {
                    animation: settings.animation,
                    states: {
                        inactive: {
                            opacity: 0.8
                        }
                    }
                }
            },
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            navigator: {
                baseSeries: 2,
                enabled: true,
                xAxis: {
                    labels: {
                        formatter: function () {
                            return this.value;
                        }
                    }
                },
                series: {
                    animation: settings.animation,
                    lineColor: chartSeriesColors.SingleValueChart.Measurement
                }
            },
            rangeSelector: {
                enabled: false
                // selected: 5  // date, month year etc
            },
            title: {
                text: settings.chartTitle,
                style: titleStyle
            },
            tooltip: siemens_tooltip, // siemensTooltip,
            xAxis: {
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                gridLineColor: siemensColors.PL_BLACK_22,
                title: {
                    text: settings.xAxisTitle,
                    style: axiesTitleStyle
                },
                labels: {
                    rotation: -90,
                    style: axiesStyle,
                    formatter: function () {
                        return series.xAxisLabels[this.value];
                    }
                },
                min: settings.navigator.start,
                max: settings.navigator.end,

                range: settings.navigator.selected
            },
            yAxis: [{
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                min: yMin, // Math.min(settings.data.result.cuSum.lowerControlLimit, settings.data.result.cuSum.minCusumMinus),
                max: yMax, // Math.max(settings.data.result.cuSum.upperControlLimit, settings.data.result.cuSum.maxCUSumPlus),
                opposite: false,
                labels: {
                    style: axiesStyle
                },
                title: {
                    text: settings.yAxisTitle,
                    style: axiesTitleStyle
                }
            }
            ],
            series: prepareSeries(series)
        };
        return optionValues;
    }

    function prepareData(series) {
        var data = settings.data,
            ucl = 0,
            lcl = 0;
        if (data && data.result && data.result.cumulatedSumValues) {
            ucl = roundFloat(data.result.cumulatedSumValues.upperControlLimit, null, true);
            lcl = roundFloat(data.result.cumulatedSumValues.lowerControlLimit, null, true);
        }
        $.each(data.subgroups, function (index, item) {
            if (SubgroupContainsStatus(item, "Eliminated") == false) {
                var customInfo = item.customInfo || [];

                series.cuSumLow.push(roundFloat(item.cuSumLow, null, true));
                series.cuSumHigh.push(roundFloat(item.cuSumHigh, null, true));
                series.cuSumUCL.push(ucl);
                series.cuSumLCL.push(lcl);
                series.cuSumCL.push(0); // the midde value should be 0

                if (customInfo.length > 0) { // It should be irrespect of xAxisCutomInfoLabel property
                    series.xAxisLabels.push(getCustomxAxisLabel(customInfo));
                }
                else {
                    series.xAxisLabels.push(index + 1); // sum start from 1
                }

                series.customInfo.push(customInfo);
            }
        });
    }

    function getCustomxAxisLabel(customInfo) {
        if (customInfo && customInfo.filter) {
            var xAxisObj = customInfo.filter(function (k) { return k.xAxisLabel === true; });
            if (xAxisObj && xAxisObj.length > 0 && xAxisObj[0].value)
                return xAxisObj[0].value;
        }
        return '';
    }

    function getCustomTooltip(customInfo) {
        var customTooltip = [];

        if (customInfo && customInfo.filter) {
            var xAxisObj = customInfo.filter(function (k) { return k.showInTooltip === true; });
            if (xAxisObj) {
                xAxisObj.forEach(function (o) {
                    customTooltip.push(o.label + ' : ' + o.value);
                });
            }
        }
        return customTooltip.join('<br>');
    }

    function prepareSeries(series) {
        var ser = [
            {
                id: seriesIds.cuSumChart.cuSumUCL, // 'CUSumUCL',
                name: getLocalizedText(seriesIds.cuSumChart.cuSumUCL, settings.locale),
                data: series.cuSumUCL,
                yAxis: 0,
                color: chartSeriesColors.CUSumChart.CUSumControlLimit,
                animation: settings.animation,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                lineWidth: 1.2,
                legendIndex: 0,
                showInLegend: shouldDisplaySeries(series.cuSumUCL),
                visible: shouldDisplaySeries(series.cuSumUCL)
            },
            {
                id: seriesIds.cuSumChart.cuSumCL, // 'CUSumCL',
                name: getLocalizedText(seriesIds.cuSumChart.cuSumCL, settings.locale),
                data: series.cuSumCL,
                animation: settings.animation,
                yAxis: 0,
                color: chartSeriesColors.CUSumChart.CUSumMeanValue,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                lineWidth: 1.2,
                legendIndex: 1,
                showInLegend: shouldDisplaySeries(series.cuSumCL),
                visible: shouldDisplaySeries(series.cuSumCL)
            },
            {
                id: seriesIds.cuSumChart.cuSumHigh, //'CUSumPlus',
                name: getLocalizedText(seriesIds.cuSumChart.cuSumHigh, settings.locale),
                data: series.cuSumHigh,
                yAxis: 0,
                animation: settings.animation,
                color: chartSeriesColors.CUSumChart.CUSumHigh,
                //step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: true,
                    radius: 4,
                    symbol: 'circle'
                },
                // lineWidth: 1.2,
                legendIndex: 2,
                showInLegend: shouldDisplaySeries(series.cuSumHigh),
                visible: shouldDisplaySeries(series.cuSumHigh)
            },
            {
                id: seriesIds.cuSumChart.cuSumLow, //  'CUSumMinus',
                name: getLocalizedText(seriesIds.cuSumChart.cuSumLow, settings.locale),
                data: series.cuSumLow,
                yAxis: 0,
                animation: settings.animation,
                color: chartSeriesColors.CUSumChart.CUSumLow,
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: true,
                    symbol: 'circle',
                    radius: 4
                },
                // lineWidth: 1.2,
                legendIndex: 3,
                showInLegend: shouldDisplaySeries(series.cuSumLow),
                visible: shouldDisplaySeries(series.cuSumLow)
            },          
            {
                id: seriesIds.cuSumChart.cuSumLCL, // 'CUSumLCL',
                name: getLocalizedText(seriesIds.cuSumChart.cuSumLCL, settings.locale),
                data: series.cuSumLCL,
                animation: settings.animation,
                yAxis: 0,
                color: chartSeriesColors.CUSumChart.CUSumControlLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                lineWidth: 1.2,
                legendIndex: 4,
                showInLegend: shouldDisplaySeries(series.cuSumLCL),
                visible: shouldDisplaySeries(series.cuSumLCL)
            }
            
        ];
        return ser;
    }

    function displayTooltip(/*tooltip*/) {
        var msg = [],
            customInfo = settings.inputs.customInfo || [];

        msg.push('<b>' + this.series.name + ' : ' + roundFloat(this.y, settings.decimalPlaces) + '</b>');

        if (customInfo.length > this.point.index && customInfo[this.point.index].length > 0) {
            msg.push('<hr style="margin-top: 7px; padding: 0; margin-bottom: -10px; "/>');
            msg.push(getCustomTooltip(customInfo[this.point.index]));
        }

        return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + msg.join('<br>') + "</span></div>";
    }

    function shouldDisplaySeries(series) {
        var notNullSeries = [];
        if (series && series.filter) {
            notNullSeries = series.filter(function (k) {
                if (!isNullOrUndefined(k))
                    return k.toString();
            });
        }

        var nonullableSeries = notNullSeries.length > 0;
        return nonullableSeries;
    }

    function setDefault() {
        if (typeof options === 'object') {
            siemensTooltip.formatter = displayTooltip;
            //if (options && typeof options === 'object') {
            if (!options.locale || !Highcharts.uiLocale[options.locale]) {
                console.error('Warning: Locale "' + options.locale + '" not found, default English locales will be used');
                options.locale = 'en';
            }
            if (options.fontSize && options.fontSize.labels) {
                axiesTitleStyle.fontSize = options.fontSize.labels;
            }
            if (options.fontSize && options.fontSize.title) {
                titleStyle.fontSize = options.fontSize.title;
            }
            //}
        }
    }
};
/**
 * @description Provide methods to draw cumulative count chart .
 *
 * @namespace CumulativeCountChart
 *
 */

/**
 * @description Draw cumulative count chart with the given parameters.
 * @memberof CumulativeCountChart
 * @function cumulativeCountChart
 * @requires highstock.js
 * @requires jQuery.js
 * @param {CumulativeCountChart.options} options - The chart options parameter
 * @example
 * $('#CUSum').cumulativeCountChart({
 *                      locale: locale,
 *                      data: {
 *                              subgroups: [
 *                                            {
 *                                            subgroupNumber: 1,
 *                                            cumulativeCount: 1,
 *                                            lowerControlLimitAbs: 39.98666666666667,
 *                                            upperControlLimitAbs: 2395.7869569035784,
 *                                            numberOfDefects: 4,
 *                                            processMeanValue: 559.8133333333333,
 *                                            statuses: 
 *                                               [ 
 *                                                  {
 *                                                      category: "LCL_1"
 *                                                  },
 *                                                  {
 *                                                      category: "ProcessViolation_1"
 *                                                  }
 *                                               ]
 *                                            }
 *                                         ]
 *                               },
 *                      decimalPlaces: 4,
 *                      height: '80%',
 *                      width: 400,
 *                      });
 *
 */

/**
* @memberof  CumulativeCountChart
* @typedef  {object} options
* @property {string} [options.locale=en]  User locale for tooltip and title etc. It require corresponding localization files to be referenced in html e.g. ui-charts-de.js should be included for german locale.
* @property {CumulativeCountChart.data} options.data - JSON Array with subgroup
* @property {CumulativeCountChart.legendSettings} legendSettings Settings for position, layout and visibility of the chart legend
* @property {string} [options.chartTitle = 'Cumulative Count Chart'] - The default value will be based on corresponding locale (if referenced).
* @property {number} [options.decimalPlaces = 4] - Number of decimal places for numbers to be shown in tooltip.
* @property {number} [options.height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
*                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
* @property {number} [options.width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
* @property {string} [options.xAxisTitle='subgroups'] - The label for x-Axis.
* @property {string} [options.yAxisTitle='Cumulative Count'] - The label for y-Axis.
* @property {boolean}  [options.xAxisCustomInfoLabel=false] - If true, the default label for xAxis will be overriden by the property of customInfo object, where xAxisLabel is true. For more information  please refer {@link CumulativeCountChart.customInfo}
* @property {boolean}  [animation=false] -Enable or disable the initial animation
* @property {CumulativeCountChart.navigator} [options.navigator]  - The navigator object is to define start and end of navigator range as well as the number of selected values
* @property {CumulativeCountChart.fontSize} [fontSize]-Option to set Fontsize of label & title.
*/

/**
 * @description the legend position settings
 * @typedef {object} legendSettings
 * @memberof CumulativeCountChart
 * @property {boolean} [legendSettings.enabled = true] - show or hide the legend.
 * @property {string} [legendSettings.align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [legendSettings.verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [legendSettings.layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [legendSettings.x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [legendSettings.Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */

/**
 * @description The navigator object is to define start and end of navigator range as well as the number of selected values
 * @typedef {object} navigator
 * @memberof CumulativeCountChart
 * @property {number} [start = undefined] - first value to be selected in the navigator when defined alone then the last n values would be selected.
 * @property {number} [end = undefined] - last value to be selected in the navigator when defined alone then the first n values would be selected
 * @property {number} [selected = 25] - the number of selected value in the navigator, if start and end are set this value has no effect.
 * @example
 * navigator: {
 *               start: 0,
 *              end: 5,
 *               selected: 5
 *           },
 */

/**
 * @description The format of data for cumulative sum chart.
 * @typedef {Array} data - The array of objects.
 * @memberof CumulativeCountChart
 * @property {Array} subgroups - The array of objects in defined format. Please look the format in the in example below. 
 * @example
 * var data = 
 * {
 *                              subgroups: [
 *                                            {
 *                                            subgroupNumber: 1,
 *                                            cumulativeCount: 1,
 *                                            lowerControlLimitAbs: 39.98666666666667,
 *                                            upperControlLimitAbs: 2395.7869569035784,
 *                                            numberOfDefects: 4,
 *                                            processMeanValue: 559.8133333333333,
 *                                            statuses: 
 *                                               [ 
 *                                                  {
 *                                                      category: "LCL_1"
 *                                                  },
 *                                                  {
 *                                                      category: "ProcessViolation_1"
 *                                                  }
 *                                               ]
 *                                            }
 *                                         ]
 *                               }
 
 */

/**
 * @description An array of objects with following properties can be injected against each subgroup.
 * @typedef {object} customInfo
 * @memberof CumulativeCountChart
 * @property {string} label - The label to be display in tooltip.
 * @property {string} value - The value to be used to show in tooltip and for xAxis labels.
 * @property {boolean} showInTooltip - if true, the label and value will also be showin in tooltip for a particular subgrup as 'label: value'
 * @example
 * <caption>A customInfo can be injected like this, if a data is defined in the given {@link CumulativeCountChart.data}</caption>

    data.subgroups[0].customInfo = [
        {
            label: 'Charge Number',
            value: 'CH001',
            showInTooltip: true,
            xAxisLabel: false
        },
        {
            label: 'Date',
            value: '05-01-2020',
            showInTooltip: true,
            xAxisLabel: true
        }
    ];
  // Note: If against a subgroup more than one objects have 'xAxisLabel' true then the value by default
  // of the first object will be taken. e.g. If in the given example both objects have xAxisLabel true
  // then in chart the value of first object on xAxis label (for subgroup 0) will be displayed.
  // Which is in this case 'CH001'
  */
/**
 * @typedef {object} fontSize
 * @memberof CumulativeCountChart
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */
$.fn.cumulativeCountChart = function (options) {
    var settings = {};
    if (options && typeof options === 'object') {
        setDefault();
        settings = $.extend({
            locale: 'en',
            data: {}, // JSON Array with subgroup
            chartTitle: getLocalizedText('cuCount_chart', options.locale),
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            xAxisTitle: getLocalizedText('x_axis_subgroups', options.locale),
            yAxisTitle: getLocalizedText('cuCount', options.locale),
            navigator: {
                start: undefined,
                end: undefined,
                selected: 25
            },
            legendSettings: {
                enabled: true,
                align: 'right',
                verticalAlign: 'top',
                layout: 'vertical',
                x: 0,
                y: 100
            },
            animation: false
        }, options);

    } else if (!options)
        throw 'Not valid options or parameter to the "chart" widget passed';
    var $this = this,
        _args = arguments,
        chartOption = createChartOptions(settings),
        chart = {
            init: function () {
                var containerId = $this.attr('id');
                Highcharts.stockChart(containerId, chartOption);
            },
            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof CumulativeCountChart
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered.
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#cumulativeCountChart').cumulativeCountChart('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64',
            *                                           width : 1800,
            *                                           height: 600,
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        };

    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function createChartOptions(settings) {
        if (!settings.data || !settings.data.subgroups)
            throw 'Invalid data format exception: Series data is not in defined format';
        var series = { cuCount: [], ucl: [], lcl: [], xbp: [], xAxisLabels: [], customInfo: [] },
            siemens_tooltip = $.extend({}, siemensTooltip);;

        prepareData(series);
        settings.inputs = series;

        var optionValues = {
            chart: {
                type: 'line',
                height: settings.height,
                width: settings.width,
                animation: settings.animation,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                }
            },
            credits: {
                enabled: false
            },
            plotOptions: {
                series: {
                    animation: settings.animation,
                    states: {
                        inactive: {
                            opacity: 0.8
                        }
                    }
                }
            },
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            navigator: {
                baseSeries: 1,
                enabled: true,
                xAxis: {
                    labels: {
                        formatter: function () {
                            return this.value;
                        }
                    }
                },
                series: {
                    animation: settings.animation,
                    lineColor: chartSeriesColors.SingleValueChart.Measurement
                }
            },
            rangeSelector: {
                enabled: false
                // selected: 5  // date, month year etc
            },
            title: {
                text: settings.chartTitle,
                style: titleStyle
            },
            tooltip: siemens_tooltip, //  siemensTooltip,
            xAxis: {
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                gridLineColor: siemensColors.PL_BLACK_22,
                title: {
                    text: settings.xAxisTitle,
                    style: axiesTitleStyle
                },
                labels: {
                    rotation: -90,
                    style: axiesStyle,
                    formatter: function () {
                        return series.xAxisLabels[this.value];
                    }
                },
                min: settings.navigator.start,
                max: settings.navigator.end,
                range: settings.navigator.selected // select last 25 samples
            },
            yAxis: [{
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                // min: yMin, // Math.min(settings.data.result.cuSum.lowerControlLimit, settings.data.result.cuSum.minCusumMinus),
                // max: yMax, // Math.max(settings.data.result.cuSum.upperControlLimit, settings.data.result.cuSum.maxCUSumPlus),
                opposite: false,
                labels: {
                    formatter: function () {
                        return roundFloat(this.value, settings.decimalPlaces); // series.xAxisLabels[this.value];
                    },
                    style: axiesStyle
                },
                title: {
                    text: settings.yAxisTitle,
                    style: axiesTitleStyle
                }
            }
            ],
            series: prepareSeries(series)
        };
        validateLegendPositioning(optionValues.legend);
        return optionValues;
    }

    function prepareData(series) {
        var data = settings.data;
        var selectedSubgroups = (data.subgroups || []).filter(function (k) {
            return k.numberOfDefects > 0;
        });

        $.each(selectedSubgroups, function (index, item) {
            if (SubgroupContainsStatus(item, "Eliminated") == false) {
                var customInfo = item.customInfo || [],
                    violations = getViolations(item.statuses);

                if (violations.newStatuses.length > 0) {
                    series.cuCount.push({
                        marker: {
                            fillColor: chartSeriesColors.ControlChart.Marker,
                            lineWidth: 3,
                            lineColor: chartSeriesColors.ControlChart.Marker,
                            symbol: 'triangle'
                        },
                        name: 'violation' + index,
                        y: roundFloat(item.cumulativeCount, null, true),
                        violations: violations.violations  //to get in tooltip and in click event
                    });
                }
                else
                    series.cuCount.push(roundFloat(item.cumulativeCount, null, true));


                series.ucl.push(roundFloat(data.result.cumulatedCountValues.upperControlLimit, null, true));
                series.lcl.push(roundFloat(data.result.cumulatedCountValues.lowerControlLimit, null, true));
                series.xbp.push(roundFloat(data.result.cumulatedCountValues.processMeanValue, null, true));

                if (settings.xAxisCustomInfoLabel === true) {
                    series.xAxisLabels.push(getCustomxAxisLabel(customInfo));
                } else {
                    series.xAxisLabels.push(roundFloat(item.subgroupNumber, null, true));
                }

                series.customInfo.push(customInfo);
            }
        });


    }

    function getViolations(statuses) {
        var data = { newStatuses: [], violations: [] };

        if (statuses && statuses.length > 0 && typeof statuses === 'object') {
            statuses.forEach(function (s) {
                if (s.category) {
                    if (s.category.toLowerCase() !== 'processviolation_1' && s.category.toLowerCase() !== 'processviolation_2')
                        data.violations.push(getLocalizedText(s.category, settings.locale));

                    data.newStatuses.push(s.category.toLowerCase());
                }
            });
        }

        if (data.newStatuses.indexOf('processviolation_1') < 0 && data.newStatuses.indexOf('processviolation_2') < 0) { // if no violations found then return empty
            data = { newStatuses: [], violations: [] };
        }

        return data;
    }

    function getCustomxAxisLabel(customInfo) {
        if (customInfo && customInfo.filter) {
            var xAxisObj = customInfo.filter(function (k) { return k.xAxisLabel === true; });
            if (xAxisObj && xAxisObj.length > 0 && xAxisObj[0].value)
                return xAxisObj[0].value;
        }
        return '';
    }

    function getCustomTooltip(customInfo) {
        var customTooltip = [];

        if (customInfo && customInfo.filter) {
            var xAxisObj = customInfo.filter(function (k) { return k.showInTooltip === true; });
            if (xAxisObj) {
                xAxisObj.forEach(function (o) {
                    customTooltip.push(o.label + ' : ' + o.value);
                });
            }
        }
        return customTooltip.join('<br>');
    }

    function prepareSeries(series) {
        var ser = [
            {
                id: seriesIds.cuCountChart.ucl, //  'CUSumMinus',
                name: getLocalizedText('UCL_1', settings.locale),

                data: series.ucl,
                yAxis: 0,
                color: chartSeriesColors.CUCountChart.controlLimit,
                animation: settings.animation,
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: false,
                    radius: 4
                },
                lineWidth: 1.2,
                legendIndex: 1,
                showInLegend: shouldDisplaySeries(series.ucl),
                visible: shouldDisplaySeries(series.ucl)
            },
            {
                id: seriesIds.cuCountChart.cuCount, //'CUSumPlus',
                name: getLocalizedText(seriesIds.cuCountChart.cuCount, settings.locale),
                data: series.cuCount,
                yAxis: 0,
                animation: settings.animation,
                color: chartSeriesColors.CUCountChart.mainSeries,
                //step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    symbol: 'circle',
                    radius: 4,
                    enabled: true
                },
                //lineWidth: 2,
                legendIndex: 2,
                showInLegend: false,
                visible: true
            },
            {
                id: seriesIds.cuCountChart.xbp, // 'CUSumLCL',
                name: getLocalizedText('xbp_1', settings.locale),
                data: series.xbp,
                animation: settings.animation,
                yAxis: 0,
                color: chartSeriesColors.CUCountChart.meanValue,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                lineWidth: 1.2,
                legendIndex: 3,
                showInLegend: shouldDisplaySeries(series.xbp),
                visible: shouldDisplaySeries(series.xbp)
            },
            {
                id: seriesIds.cuCountChart.lcl, // 'CUSumUCL',
                name: getLocalizedText('LCL_1', settings.locale),
                data: series.lcl,
                animation: settings.animation,
                yAxis: 0,
                color: chartSeriesColors.CUCountChart.controlLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                lineWidth: 1.2,
                legendIndex: 4,
                showInLegend: shouldDisplaySeries(series.lcl),
                visible: shouldDisplaySeries(series.lcl)
            }
        ];
        return ser;
    }

    function displayTooltip(/*tooltip*/) {
        var msg = [],
            customInfo = settings.inputs.customInfo || [];

        msg.push('<b>' + this.series.name + ' : ' + roundFloat(this.y, settings.decimalPlaces) + '</b>');

        if (this.key.toString().startsWith('violation') && this.point.options.violations) {
            msg.push('<hr style="margin-top: 7px; padding: 0; margin-bottom: -10px; "/>');
            this.point.options.violations.forEach(function (v) { if(v) msg.push(v); });
        }

        if (customInfo.length > this.point.index && customInfo[this.point.index].length > 0) {
            msg.push('<hr style="margin-top: 7px; padding: 0; margin-bottom: -10px; "/>');
            msg.push(getCustomTooltip(customInfo[this.point.index]));
        }

        return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + msg.join('<br>') + "</span></div>";

    }

    function shouldDisplaySeries(series) {
        var notNullSeries = [];
        if (series && series.filter) {
            notNullSeries = series.filter(function (k) {
                if (!isNullOrUndefined(k))
                    return k.toString();
            });
        }

        var nonullableSeries = notNullSeries.length > 0;
        return nonullableSeries;
    }

    function setDefault() {
        siemensTooltip.formatter = displayTooltip;
        if (options && typeof options === 'object') {
            if (!options.locale || !Highcharts.uiLocale[options.locale]) {
                console.error('Warning: Locale "' + options.locale + '" not found, default English locales will be used');
                options.locale = 'en';
            }

            if (options.fontSize && options.fontSize.labels) {
                axiesTitleStyle.fontSize = options.fontSize.labels;
            }
            if (options.fontSize && options.fontSize.title) {
                titleStyle.fontSize = options.fontSize.title;
            }

        }
    }
};
/**
 * @description Provide methods to draw cumulative sum chart .
 * @namespace uCuSumChart
 */

/**
 * @description Draw cumulative sum chart with the given parameters.
 * @memberof uCuSumChart
 * @function cumulatedUSumChart
 * @requires highstock.js
 * @requires jQuery.js
 * @param {uCuSumChart.options} options - The chart options parameter
 */

/** 
 * @memberof uCuSumChart
 * @typedef  {object} options
 * @property {string} [options.locale=en]  User locale for tooltip and title etc. It require corresponding localization files to be referenced in html e.g. ui-charts-de.js should be included for german locale.
 * @property {uCuSumChart.data} options.data - JSON Array with subgroup
 * @property {string} [options.chartTitle = 'Cumulative Sum Chart'] - The default value will be based on corresponding locale (if referenced).
 * @property {number} [options.decimalPlaces = 4] - Number of decimal places for numbers to be shown in tooltip.
 * @property {number} [options.height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number} [options.width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string} [options.xAxisTitle='subgroups'] - The label for x-Axis.
 * @property {string} [options.yAxisTitle='Cumulative Sum'] - The label for y-Axis.
 * @property {boolean}  [options.xAxisCustomInfoLabel=false] - If true, the default label for xAxis will be overriden by the property of customInfo object, where xAxisLabel is true. For more information  please refer {@link CumulativeCountChart.customInfo}
 * @property {uCuSumChart.legendSettings} legendSettings Settings for position, layout and visibility of the chart legend
 * @property {boolean}  [animation=false] -Enable or disable the initial animation
 * @property {uCuSumChart.navigator} [options.navigator]  - The navigator object is to define start and end of navigator range as well as the number of selected values
 * @property {uCuSumChart.fontSize} [fontSize]-Option to set Fontsize of label & title.
 */ 
 
/**
 * @description the values to set the legend to the prefered position or to hide it totaly
 * @typedef {object} legendSettings
 * @memberof uCuSumChart
 * @property {boolean} [enabled = true] - show or hide the legend.
 * @property {string} [align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */

/**
 * @description The format of data for cumulative sum chart.
 * @typedef {Array} data - The array of objects.
 * @memberof uCuSumChart
 * @property {Array} subgroups - The array of objects in defined format. Please look the format in the in example below.
 */

/**
 * @description The navigator object is to define start and end of navigator range as well as the number of selected values
 * @typedef {object} navigator
 * @memberof uCuSumChart
 * @property {number} [start = undefined] - first value to be selected in the navigator when defined alone then the last n values would be selected.
 * @property {number} [end = undefined] - last value to be selected in the navigator when defined alone then the first n values would be selected
 * @property {number} [selected = 25] - the number of selected value in the navigator, if start and end are set this value has no effect.
 * @example
 * navigator: {
 *               start: 0,
 *              end: 5,
 *               selected: 5
 *           },
 */

/**
 * @description An array of objects with following properties can be injected against each subgroup.
 * @typedef {object} customInfo
 * @memberof uCuSumChart
 * @property {string} label - The label to be display in tooltip.
 * @property {string} value - The value to be used to show in tooltip and for xAxis labels.
 * @property {boolean} showInTooltip - if true, the label and value will also be showin in tooltip for a particular subgrup as 'label: value'
  * @example
 * <caption>A customInfo can be injected like this, if a data is defined in the given {@link CumulativeCountChart.data}</caption>

    data.subgroups[0].customInfo = [
        {
            label: 'Charge Number',
            value: 'CH001',
            showInTooltip: true,
            xAxisLabel: false
        },
        {
            label: 'Date',
            value: '05-01-2020',
            showInTooltip: true,
            xAxisLabel: true
        }
    ];
  // Note: If against a subgroup more than one objects have 'xAxisLabel' true then the value by default
  // of the first object will be taken. e.g. If in the given example both objects have xAxisLabel true
  // then in chart the value of first object on xAxis label (for subgroup 0) will be displayed.
  // Which is in this case 'CH001'
  */
/**
 * @typedef {object} fontSize
 * @memberof uCuSumChart
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */

$.fn.cumulatedUSumChart = function (options) {
    var settings = {};
    if (options && typeof options === 'object') {
        setDefault();
        settings = $.extend({
            locale: 'en',
            data: {}, // JSON Array with subgroup
            chartTitle: getLocalizedText('CUSum_chart', 'en'),
            decimalPlaces: 2,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            xAxisTitle: getLocalizedText('x_axis_subgroups', 'en'),
            yAxisTitle: undefined,
            navigator: {
                start: undefined,
                end: undefined,
                selected: 25
            },
            legendSettings: {
                enabled: true,
                align: 'right',
                verticalAlign: 'top',
                layout: 'vertical',
                x: 0,
                y: 100
            },
            animation: false
        }, options);

    } else if (!options)
        throw 'Not valid options or parameter to the "chart" widget passed';


    var $this = this,
        _args = arguments,
        chart = {
            init: function () {
                var containerId = $this.attr('id');
                Highcharts.stockChart(containerId, chartOption);
            },
            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof uCuSumChart
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered.
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#cumulatedUSumChart').cumulatedUSumChart('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64',
            *                                           width : 1800,
            *                                           height: 600,
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        },
        chartOption = createChartOptions(settings);
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function createChartOptions(settings) {
        if (!settings.data || !settings.data.subgroups)
            throw 'Invalid data format exception: Series data is not in defined format';
        var series = {
            CUSumMinus: [],
            CUSumPlus: [],
            CUSum: [],
            CUSumUCL: [],
            CUSumLCL: [],
            CUSumCL: [],
            xAxisLabels: []
        },
            siemens_tooltip = $.extend({}, siemensTooltip);
        // var ymin = Math.min(settings.data.result.cumulatedSumValues.lowerControlLimit, settings.data.result.cumulatedSumValues.minCumulatedSumLow);
        // var ymax = Math.max(settings.data.result.cumulatedSumValues.upperControlLimit, settings.data.result.cumulatedSumValues.maxCumulatedSumHigh);
        // console.log(ymin, ymax);
        prepareData(settings.data, series);
        var optionValues = {
            chart: {
                type: 'line',
                height: settings.height,
                width: settings.width,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                }
            },
            credits: {
                enabled: false
            },
            plotOptions: {
                series: {
                    animation: settings.animation,
                    states: {
                        inactive: {
                            opacity: 0.8
                        }
                    }
                }
            },
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            navigator: {
                baseSeries: 0,
                enabled: true,
                xAxis: {
                    labels: {
                        formatter: function () {
                            return this.value;
                        }
                    }
                },
                series: {
                    animation: settings.animation,
                    lineColor: chartSeriesColors.SingleValueChart.Measurement
                }
            },
            rangeSelector: {
                enabled: false
                // selected: 5  // date, month year etc
            },
            title: {
                text: settings.chartTitle,
                style: titleStyle
            },
            tooltip: siemens_tooltip, // siemensTooltip,
            xAxis: {
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                gridLineColor: siemensColors.PL_BLACK_22,
                title: {
                    text: settings.xAxisTitle, // getLocalizedText('x_axis_subgroups', settings.locale),
                    style: axiesTitleStyle
                },
                labels: {
                    style: axiesStyle,
                    formatter: function () {
                        return series.xAxisLabels[this.value];
                    }
                },
                min: settings.navigator.start,
                max: settings.navigator.end,

                range: settings.navigator.selected
            },
            yAxis: [{
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                min: Math.min(settings.data.result.cumulatedSumValues.lowerControlLimit, settings.data.result.cumulatedSumValues.minCumulatedSumLow),
                max: Math.max(settings.data.result.cumulatedSumValues.upperControlLimit, settings.data.result.cumulatedSumValues.maxCumulatedSumHigh),
                opposite: false,                
                labels: {
                    formatter: function () {
                        return roundFloat(this.value, settings.decimalPlaces); // series.xAxisLabels[this.value];
                    },
                    style: axiesStyle
                },
                title: {
                    text: settings.yAxisTitle,
                    style: axiesTitleStyle
                }
            }
            ],
            series: prepareSeries(series)
        };
        validateLegendPositioning(optionValues.legend);
        return optionValues;
    }

    function prepareData(data, series) {
        
        $.each(data.subgroups, function (index, item) {
            
            if (SubgroupContainsStatus(item, "Eliminated") == false ) {
                series.CUSumMinus.push(item.cuSumLow);
                series.CUSumPlus.push(item.cuSumHigh);
                series.CUSum.push(item.cuSum);
                series.CUSumUCL.push(data.result.cumulatedSumValues.upperControlLimit);
                series.CUSumLCL.push(data.result.cumulatedSumValues.lowerControlLimit);
                series.CUSumCL.push(0);
                series.xAxisLabels.push(item.subgroupNumber);
            }
        });


    }

    function prepareSeries(series) {
        var ser = [
            {
                id: 'CUSumMinus',
                name: getLocalizedText('CUSumLow', settings.locale),
                data: series.CUSumMinus,
                yAxis: 0,
                color: chartSeriesColors.CUSumChart.CUSumLow,
                animation: settings.animation,
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: true,
                    radius: 4
                },
                lineWidth: 2,
                legendIndex: 1,
                showInLegend: true,
                visible: true
            },
            {
                id: 'CUSumPlus',
                name: getLocalizedText('CUSumHigh', settings.locale),
                data: series.CUSumPlus,
                yAxis: 0,
                color: chartSeriesColors.CUSumChart.CUSumHigh,
                //step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                marker: {
                    enabled: true,
                    radius: 4
                },
                animation: settings.animation,
                lineWidth: 2,
                legendIndex: 2,
                showInLegend: true,
                visible: true
            },
            {
                id: 'CUSum',
                name: getLocalizedText('CUSum', settings.locale),
                data: series.CUSum,
                yAxis: 0,
                color: chartSeriesColors.CUSumChart.CUSumHigh,
                //step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                animation: settings.animation,
                marker: {
                    enabled: true,
                    radius: 4
                },
                lineWidth: 2,
                legendIndex: 3,
                showInLegend: true,
                visible: true
            },
            {
                id: 'CUSumUCL',
                name: getLocalizedText('CUSumUCL', settings.locale),
                data: series.CUSumUCL,
                yAxis: 0,
                animation: settings.animation,
                color: chartSeriesColors.CUSumChart.CUSumControlLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                lineWidth: 2,
                legendIndex: 4,
                showInLegend: true,
                visible: true
            },
            {
                id: 'CUSumLCL',
                name: getLocalizedText('CUSumLCL', settings.locale),
                data: series.CUSumLCL,
                yAxis: 0,
                animation: settings.animation,
                color: chartSeriesColors.CUSumChart.CUSumControlLimit,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                lineWidth: 2,
                legendIndex: 5,
                showInLegend: true,
                visible: true
            },
            {
                id: 'CUSumCL',
                animation: settings.animation,
                name: getLocalizedText('CUSumCL', settings.locale),
                data: series.CUSumCL,
                yAxis: 0,
                color: chartSeriesColors.CUSumChart.CUSumMeanValue,
                step: 'left',
                tooltip: {
                    valueDecimals: settings.decimalPlaces
                },
                lineWidth: 2,
                legendIndex: 6,
                showInLegend: true,
                visible: true
            }
        ];
        return ser;
    }

    function displayTooltip(/*tooltip*/) {
        var msg = [];

        msg.push('<b>' + this.series.name + ' : ' + roundFloat(this.y, settings.decimalPlaces) + '</b>');
        return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + msg.join('<br>') + "</span></div>";

    }

    function setDefault() {
        if (typeof options === 'object') {
            siemensTooltip.formatter = displayTooltip;
            if (options && typeof options === 'object') {
                if (!options.locale || !Highcharts.uiLocale[options.locale]) {
                    console.error('Warning: Locale "' + options.locale + '" not found, default English locales will be used');
                    options.locale = 'en';
                }
                if (options.fontSize && options.fontSize.labels) {
                    axiesTitleStyle.fontSize = options.fontSize.labels;
                }
                if (options.fontSize && options.fontSize.title) {
                    titleStyle.fontSize = options.fontSize.title;
                }
            }
        }
    }
};
/**
 * @description Provide methods for drawing Multi variable control chart .
 *
 * @namespace MultiVarControlChart
 * */
/**
 * @description Draw a Multi variable  chart using the provided data from the options parameter, this function can draw up to n variables with their Upper and Lower control limits 
 * @memberof MultiVarControlChart
 * @function multiVarControlChart
 * @requires highstock.js
 * @requires jQuery.js
 * @param {MultiVarControlChart.options} options - The chart options parameter
 * @example
 *  $('#mVarCChart').multiVarControlChart({
            locale: locale,
            data: data, // JSON Array with Variables
            chartTitle: 'MultiVarControlChart',
            xAxisTitle: 'SubGroups',
            decimalPlaces: 4,
            height: 600
        });
*/
/**
 * @description the legend position settings
 * @typedef {object} legendSettings
 * @memberof MultiVarControlChart
 * @property {boolean} [legendSettings.enabled = true] - show or hide the legend.
 * @property {string} [legendSettings.align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [legendSettings.verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [legendSettings.layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [legendSettings.x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [legendSettings.Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */

/**
 * @description The navigator object is to define start and end of navigator range as well as the number of selected values
 * @typedef {object} navigator
 * @memberof MultiVarControlChart
 * @property {number} [start = undefined] - first value to be selected in the navigator when defined alone then the last n values would be selected.
 * @property {number} [end = undefined] - last value to be selected in the navigator when defined alone then the first n values would be selected
 * @property {number} [selected = 25] - the number of selected value in the navigator, if start and end are set this value has no effect.
 * @example
 * navigator: {
 *               start: 0,
 *              end: 5,
 *               selected: 5
 *           },
 */

/**
 * @memberof MultiVarControlChart
 * @typedef {object} options
 * @property {MultiVarControlChart.data} data  Array of variables up to n Variables each variable contains subgroups measurements and all other info   {@link pieChart.data}
 * @property {string} [options.locale=en]  User locale         
 * @property {MultiVarControlChart.data} options.data - JSON Array with Variables , specifications
 * @property {MultiVarControlChart.legendSettings} legendSettings - Settings for position, layout and visibility of the chart legend
 * @property {string} options.chartTitle The chart's main title.
 * @property {number} [options.decimalPlaces = 4] - The number of digits to appear after the decimal point.
 * @property {number} [options.height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number} [options.width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string} [options.xAxisTitle=null] - The x-Axis label displayed underneath the x-axis.
 * @property {boolean}  [animation=false] -Enable or disable the initial animation
 * @property {MultiVarControlChart.navigator} [options.navigator]  - The navigator object is to define start and end of navigator range as well as the number of selected values
 * @property {MultiVarControlChart.fontSize} [fontSize]-Option to set Fontsize of label & title.
 */

/**
 * @typedef {object} fontSize
 * @memberof MultiVarControlChart
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */
$.fn.multiVarControlChart = function (options) {
    var settings = {},
        tooltipRow = '<tr series_id="{2}"><td style="text-align: left; padding: 0;">{0}</td> <td style="text-align: right; padding: 0;">{1}</td></tr>',
        tooltipHr = '<tr><td colspan="2;" style = "padding: 3px 4px 3px 4px;"><hr style="margin-top: 0; padding: 0; margin-bottom: 0;"/></td></tr>',
        tooltipTableStart = '<table style="border-spacing: 0px;"><tbody>',
        tooltipTableEnd = '</tbody></table>';

    if (options && typeof options === 'object') {
        var _options = setDefault(options); // extend the object and set default

        //settings = $.extend({
        //    locale: 'en',
        //    data: {}, // JSON Array with subgroup
        //    chartTitle: undefined,
        //    xAxisTitle: undefined,
        //    decimalPlaces: 4,
        //    height: undefined,
        //    width: undefined,
        //    yAxisUnit: getDefaultyAxisUnitObj(),
        //    legendSettings: {
        //        enabled: true,
        //        align: 'right',
        //        verticalAlign: 'top',
        //        layout: 'vertical',
        //        x: 0,
        //        y: 100
        //    },
        //    isSeries2Visible: !(_options.chartType === chartTypes.xb || _options.chartType === chartTypes.s || _options.chartType === chartTypes.R || _options.chartType === chartTypes.mR || _options.chartType === chartTypes.ms || _options.chartType === chartTypes.med || _options.chartType === chartTypes.mxb || _options.chartType === chartTypes.x),
        //    animation: false,
        //    siemensTooltip: {}
        //}, _options);

        settings = $.extend({
            chartType: chartTypes.xb_s, // default XB Chart
            chartName: options.chartType, // save the chart name e.g. xb_s etc.
            locale: 'en',
            data: {}, // JSON Array with subgroup
            chartTitle: getLocalizedText(options.chartType + '_chart', _options.locale),
            xAxisTitle: getLocalizedText('x_axis_subgroups', _options.locale),
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            backgroundColor: undefined,
            yAxisScale: {
                min: undefined,
                max: undefined,
                minScaling: undefined,
                maxScaling: undefined
            },
            navigator: {
                start: undefined,
                end: undefined,
                selected: 25
            },

            hiddenSeries: [], 
            xAxisCustomInfoLabel: false,
            boxplot: false,
            showMeasurements: false,
            yAxisUnit: getDefaultyAxisUnitObj(),
            isSeries2Visible: !(_options.chartType === chartTypes.xb || _options.chartType === chartTypes.s || _options.chartType === chartTypes.R || _options.chartType === chartTypes.mR || _options.chartType === chartTypes.ms || _options.chartType === chartTypes.med || _options.chartType === chartTypes.mxb || _options.chartType === chartTypes.x),

            /*Event Functions*/
            onLegendClick: undefined,
            onSeriesClick: undefined,
            onChartLoaded: undefined,

            siemensTooltip: {},
            legendSettings: {
                enabled: undefined,
                align: undefined,
                verticalAlign: undefined,
                layout: undefined,
                x: undefined,
                y: undefined
            },
            enableTolerenceLimits: false,
            animation: false
        }, _options);




    } else if (typeof options === 'string') { // Method call
        settings = $(this).data('settings') || {}; // restore the settings object from prev settings
    } else if (!options)
        throw 'Not valid options or parameter to the "chart" extension passed';
    var $this = this,
        _args = arguments,
        chart = {
            init: function () {
                var containerId = $this.attr('id');

                if (settings.data && settings.data.variablesResponse) {
                    var seriesObj = parseResponse(settings.data.variablesResponse, settings.isSingleSpec);
                    var _chart = Highcharts.stockChart(containerId, prepareChartOptions(seriesObj));
                    $this.data('chartApi', _chart);
                    $this.data('settings', settings);
                } else
                    throw 'Invalid data format exception: Series data is not in defined format';

            },

            /**
             * @description - Filter chart Series based on a filter input, if the filter parameter is null or one of it's properties has no value the function will automatically reset the filter
             * @memberof MultiVarControlChart
             * @function filter
             * @param {object} filter - Filter Object parameter is an object has tow properties label & Value in where value is comma separated string represent multiple values  
             * @example
             *   to filter series according to info that in Custom Info Obj
             *   $('#mVarCChart').multiVarControlChart('filter', {label:'Equipment',value:'A;B;C'});
             */
            filter: function (filter) {
                var reset = false;
                if (!filter || isEmpty(filter.label) || isEmpty(filter.value))
                    reset = true;
                var chart = $this.data('chartApi');
                var valuesArray = filter.value.split(';');
                var variablesResponse = settings.data.variablesResponse
                if (variablesResponse) {
                    $.each(variablesResponse, function (index, variable) {
                        var varID = variable.variableId;
                        var target = 1;
                        var ucl = [];
                        var lcl = [];
                        var mainVar = [];
                        if (variable && variable.specifications)
                            target = roundFloat(variable.specifications.target, false);
                        $.each(variable.subgroups, function (i, subGroup) {
                            if (subGroup.customInfo && subGroup.customInfo.filter(function (obj) {
                                return obj.label === filter.label && valuesArray.indexOf(obj.value) > -1;

                            }).length > 0 || reset) {

                                ucl.push(normalize(subGroup.upperControlLimit1Abs, target));
                                lcl.push(normalize(subGroup.lowerControlLimit1Abs, target));
                                mainVar.push(normalize(subGroup.calculatedXb, target));
                                console.log(subGroup);
                            }

                        });
                        chart.get(seriesIds.controlChart.ucl1 + '_' + varID).update({
                            data: ucl
                        }, false, false, false);

                        chart.get(seriesIds.controlChart.lcl1 + '_' + varID).update({
                            data: lcl
                        }, false, false, false);
                        chart.get(varID).update({
                            data: mainVar
                        }, false, false, false);

                    });
                    chart.redraw();

                }
            },

            /**
             * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
             * @memberof MultiVarControlChart
             * @function createBase64Image
             * @param {object} options - An object with specified properties
             * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
             * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
             * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
             * @param  {Function} options.success - Callback when chart is rendered.
             * @param {Function} [options.error] - Callback to get error information.
             * @requires exporting.js
             * @requires offline-exporting.js
             * @requires canvg.js - For Internet explorer only
             * @example
             * $('#multiVarControlChart').multiVarControlChart('createBase64Image',
             *                                       {
             *                                           containerId : 'sImageBase64',
             *                                           width : 1800,
             *                                           height: 600,
             *                                           success: function(){
             *                                                               console.log(document.getElementById('sImageBase64').value);
             *                                           },
             *                                           error: function(e) {
             *                                                               console.log(e);
             *                                           }
             *                                       });
             */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        };
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2], _args[3]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function getDefaultyAxisUnitObj() {
        var obj = {
            text: '',
            x: 60,
            y: undefined,
            fontSize: undefined
        };
        return obj;
    }


    function parseResponse(response, isSingleSpec) {
        var series = [];
        var seriesIndex = 0;
        var bordersOnlyOneTime = isSingleSpec;
        // var colors = [];
        // colors = Object.values(chartSeriesColors.multiVarControlChart);
        var colors = $.map(chartSeriesColors.multiVarControlChart, function (value, key) { return value; }) || [];
        var colorIndex = 0;

        $.each(response, function (index, variable) {
            var target = 1;
            if (variable && variable.specifications && variable.specifications.target)
                target = roundFloat(variable.specifications.target, false);
            var ucl = [];
            var lcl = [];
            var ucl2 = [];
            var lcl2 = [];
            var utl = [];
            var ltl = [];
            var mainVar = [];
            var series2 = [];
            var xbp2 = [];

            $.each(variable.subgroups, function (i, subgroup) {
                if ((bordersOnlyOneTime && index === 0) || !bordersOnlyOneTime) {
                    ucl.push(normalize(subgroup.upperControlLimit1Abs, target));
                    lcl.push(normalize(subgroup.lowerControlLimit1Abs, target));
                    ucl2.push(normalize(subgroup.upperControlLimit2Abs, target));
                    lcl2.push(normalize(subgroup.lowerControlLimit2Abs, target));
                    utl.push(normalize(subgroup.upperToleranceLimitAbs, target));
                    ltl.push(normalize(subgroup.lowerToleranceLimitAbs, target));
                }
                mainVar.push(normalize(subgroup.calculatedXb, target));
                xbp2.push(normalize(subgroup.xpb2, target));
                series2.push(settings.chartType === chartTypes.xb_s || settings.chartType === chartTypes.mxb_ms || settings.chartType === chartTypes.x_ms ? roundFloat(subgroup.calculatedS, true) : roundFloat(subgroup.calculatedR, true));
            });


            if (bordersOnlyOneTime && index === 0) {
                series.push(createSeriesObj({
                    id: seriesIds.controlChart.ucl1 + '_' + variable.variableId,
                    name: getLocalizedText('UCL', settings.locale),
                    data: ucl,
                    index: seriesIndex++,
                    color: chartSeriesColors.multiVarControlChart.ControlLimit,
                    step: 'left',
                    yAxis: 0
                }));
                series.push(createSeriesObj({
                    id: seriesIds.controlChart.lcl1 + '_' + variable.variableId,
                    name: getLocalizedText('LCL', settings.locale),
                    data: lcl,
                    index: seriesIndex++,
                    color: chartSeriesColors.multiVarControlChart.ControlLimit,
                    step: 'left',
                    yAxis: 0
                }));
                series.push(createSeriesObj({
                    id: seriesIds.controlChart.utl1 + '_' + variable.variableId,
                    name: getLocalizedText('UTL', settings.locale),
                    data: utl,
                    color: chartSeriesColors.multiVarControlChart.ToleranceLimit,
                    index: seriesIndex++,
                    step: 'left',
                    yAxis: 0
                }));
                series.push(createSeriesObj({
                    id: seriesIds.controlChart.ltl1 + '_' + variable.variableId,
                    name: getLocalizedText('LTL', settings.locale),
                    data: ltl,
                    index: seriesIndex++,
                    color: chartSeriesColors.multiVarControlChart.ToleranceLimit,
                    step: 'left',
                    yAxis: 0
                }));
            } else if (bordersOnlyOneTime == false) {
                series.push(createSeriesObj({
                    id: seriesIds.controlChart.ucl1 + '_' + variable.variableId,
                    name: getLocalizedText('UCL_1', settings.locale) + '_' + variable.variableId,
                    data: ucl,
                    index: seriesIndex++,
                    step: 'left',
                    yAxis: 0
                }));
                series.push(createSeriesObj({
                    id: seriesIds.controlChart.lcl1 + '_' + variable.variableId,
                    name: getLocalizedText('LCL_1', settings.locale) + '_' + variable.variableId,
                    data: lcl,
                    index: seriesIndex++,
                    step: 'left',
                    yAxis: 0
                }));
                series.push(createSeriesObj({
                    id: seriesIds.controlChart.utl1 + '_' + variable.variableId,
                    name: getLocalizedText('UTL_1', settings.locale) + '_' + variable.variableId,
                    data: utl,
                    index: seriesIndex++,
                    step: 'left',
                    yAxis: 0
                }));
                series.push(createSeriesObj({
                    id: seriesIds.controlChart.ltl1 + '_' + variable.variableId,
                    name: getLocalizedText('LTL_1', settings.locale) + '_' + variable.variableId,
                    data: ltl,
                    index: seriesIndex++,
                    step: 'left',
                    yAxis: 0
                }));
            }

            if (settings.isSeries2Visible && bordersOnlyOneTime && index === 0) {
                series.push(createSeriesObj({
                    id: seriesIds.controlChart.ucl2 + '_' + variable.variableId,
                    linkedTo: seriesIds.controlChart.ucl1 + '_' + variable.variableId,
                    color: chartSeriesColors.multiVarControlChart.ControlLimit,
                    name: getLocalizedText('UCL_2', settings.locale),
                    data: ucl2,
                    index: seriesIndex++,
                    step: 'left',
                    showInLegend: false,
                    yAxis: 1
                }));

                series.push(createSeriesObj({
                    id: seriesIds.controlChart.lcl2 + '_' + variable.variableId,
                    color: chartSeriesColors.multiVarControlChart.ControlLimit,
                    linkedTo: seriesIds.controlChart.lcl1 + '_' + variable.variableId,
                    name: getLocalizedText('LCL_2', settings.locale),
                    data: lcl2,
                    index: seriesIndex++,
                    step: 'left',
                    showInLegend: false,
                    yAxis: 1
                }));
            }

            series.push(createSeriesObj({
                id: variable.variableId,
                name: variable.variableId,
                data: mainVar,
                yAxis: 0,
                visible: true,
                showInLegend: true,
                index: seriesIndex,
                color: colors[colorIndex],
                showInNavigator: true,
                showCustomInfo: true
            }));
            //baseSeries.push(series.length - 1);

            if (settings.isSeries2Visible) {
                series.push(createSeriesObj({
                    id: variable.variableId + ' ' + '(CC2)',
                    linkedTo: variable.variableId,
                    name: variable.variableId,
                    data: series2,
                    yAxis: settings.isSeries2Visible ? 1 : 0,
                    visible: settings.isSeries2Visible,
                    showInLegend: false,
                    step: false,
                    index: seriesIndex,
                    color: colors[colorIndex],
                    showCustomInfo: false
                }));
            }
            colorIndex++;
            if (colorIndex > 19) colorIndex = 0;

        });
        return series;
    }

    function prepareChartOptions(seriesData) {
        var siemensTooltip = $.extend({}, siemensTooltip);

        var options = {
            chart: {
                type: 'line',
                height: settings.height,
                width: settings.width,
                animation: settings.animation,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                }
            },
            credits: {
                enabled: false
            },
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            plotOptions: {
                series: {
                    states: {
                        inactive: {
                            opacity: 0.8
                        }
                    },
                    animation: settings.animation,
                }
            },
            navigator: {
                baseSeries: 6,
                enabled: true, // to show or hide zoom level on x-axis
                series: {
                    lineColor: chartSeriesColors.ControlChart.Measurement
                },
                xAxis: {
                    labels: {
                        formatter: function () {
                            return this.value;
                        }
                    }
                }
            },
            rangeSelector: {
                enabled: false
            },
            title: {
                text: settings.chartTitle,
                style: titleStyle
            },
            tooltip: settings.siemensTooltip, // siemensTooltip,
            //tooltip: {enabled: false},
            xAxis: {
                title: {
                    text: settings.xAxisTitle,
                    style: axiesTitleStyle
                },
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 1,
                gridLineColor: siemensColors.PL_BLACK_22,
                labels: {
                    rotation: -90,
                    style: axiesStyle,
                    formatter: function () {
                        return this.value;
                    }
                },
                min: settings.navigator.start,
                max: settings.navigator.end,

                range: settings.navigator.selected
            },
            yAxis: yAxisLabels(),
            series: seriesData
        };
        validateLegendPositioning(options.legend);
        options.tooltip.shared = false;
        return options;
    }
    // multi var control chart 
    function yAxisLabels() {
        var unit = settings.yAxisUnit && isNullOrUndefined(settings.yAxisUnit.text) ? '' : settings.yAxisUnit.text;
        var yAxis = [{
            //min: settings.series1yAxisMinValue,
            //max: settings.series1yAxisMaxValue,
            min: getYaxis1MinScaling(),
            max: getYaxis1MaxScaling(),
            opposite: false,
            lineWidth: 2,
            labels: {
                // rotation: -90, // settings.xAxisLabelsRotation,
                formatter: function () {
                    return roundFloat(this.value, settings.decimalPlaces);
                },
                style: axiesStyle
            },
            title: {
                text: settings.yAxisSeries1Title,
                style: axiesTitleStyle
            },
            lineColor: siemensColors.PL_BLACK_22,
        }];

        if (settings.isSeries2Visible) {
            yAxis[0].height = '53%';
            yAxis.push({
                //min: settings.series2yAxisMinValue,
                //max: settings.series2yAxisMaxValue,
                min: getYaxis2MinScaling(),
                max: getYaxis2MaxScaling(),

                labels: {
                    // align: 'right',
                    // x: -8
                    formatter: function () {
                        return roundFloat(this.value, settings.decimalPlaces);
                    },
                    style: axiesStyle
                },
                title: {
                    text: settings.yAxisSeries2Title,
                    style: axiesTitleStyle
                },
                opposite: false,
                top: '53%',
                height: '46%',
                offset: 0,
                lineWidth: 2,
                lineColor: siemensColors.PL_BLACK_22
            });
        }

        yAxis.push({ // to show units of chart
            opposite: false,
            title: {
                reserveSpace: false,
                text: unit,
                align: 'high',
                rotation: 0,
                x: settings.yAxisUnit.x,
                y: settings.yAxisUnit.y,
                style: {
                    fontSize: settings.yAxisUnit.fontSize,
                    color: siemensColors.PLBlack4
                }
            }
        });
        return yAxis;
    }

    function getYaxis1MinScaling() {
        if (!isNullOrUndefined(settings.yAxisScale)) {
            if (!isNullOrUndefined(settings.yAxisScale.min))
                return settings.yAxisScale.min;
            if (!isNullOrUndefined(settings.yAxisScale.minScaling))
                return getYaxis1MinValue(settings.yAxisScale.minScaling)
        }
        return settings.series1yAxisMinValue;
    }

    function getYaxis1MaxScaling() {
        if (!isNullOrUndefined(settings.yAxisScale)) {
            if (!isNullOrUndefined(settings.yAxisScale.max))
                return settings.yAxisScale.max;
            if (!isNullOrUndefined(settings.yAxisScale.maxScaling))
                return getYaxis1MaxValue(settings.yAxisScale.maxScaling)

        }
        return settings.series1yAxisMaxValue;
    }
    // multi var control chart 
    function getYaxis2MinScaling() {
        if (!isNullOrUndefined(settings.yAxisScale)) {
            if (!isNullOrUndefined(settings.yAxisScale.secondMin))
                return settings.yAxisScale.secondMin;
            if (!isNullOrUndefined(settings.yAxisScale.secondMinScaling))
                return getYaxis2MinValue(settings.yAxisScale.secondMinScaling)
        }
        return settings.series2yAxisMinValue;
    }

    function getYaxis2MaxScaling() {
        if (!isNullOrUndefined(settings.yAxisScale)) {
            if (!isNullOrUndefined(settings.yAxisScale.secondMax))
                return settings.yAxisScale.secondMax;
            if (!isNullOrUndefined(settings.yAxisScale.secondMaxScaling))
                return getYaxis2MaxValue(settings.yAxisScale.secondMaxScaling)

        }
        return settings.series2yAxisMaxValue;
    }

    function getYaxis2MinValue(scalingType) {
        var minValue = null;
        if (scalingType == scaling.controlLimit) {
            var lcl = settings.inputs.LCL2.reduce(function (a, b) {
                return Math.min(a, b);
            });
            var ucl = settings.inputs.UCL2.reduce(function (a, b) {
                return Math.min(a, b);
            });
            minValue = Math.min(lcl, ucl);
        }
        return minValue;
    }

    function getYaxis2MaxValue(scalingType) {
        var maxValue = null;
        if (scalingType == scaling.controlLimit) {
            var lcl = settings.inputs.LCL2.reduce(function (a, b) {
                return Math.max(a, b);
            });
            var ucl = settings.inputs.UCL2.reduce(function (a, b) {
                return Math.max(a, b);
            });
            maxValue = Math.max(lcl, ucl);
        }
        return maxValue;
    }
    //multiVarControlChart
    function setDefault(_options) {

        var opt = undefined;
        if (_options && typeof _options === 'object') {

            opt = $.extend({}, _options);

            opt.siemensTooltip = $.extend({}, siemensTooltip);
            opt.siemensTooltip.formatter = displayTooltip;
            opt.siemensTooltip.split = false;

            opt.chartType = parseChartType(opt.chartType);
            if (!opt.locale || !Highcharts.uiLocale[opt.locale]) {
                console.error('Warning: Locale "' + opt.locale + '" not found, default English locales will be used');
                opt.locale = 'en';
            }

            if (_options.fontSize && _options.fontSize.labels) {
                axiesTitleStyle.fontSize = _options.fontSize.labels;
            }
            if (_options.fontSize && _options.fontSize.title) {
                titleStyle.fontSize = _options.fontSize.title;
            }

        }

        return opt;
    }

    function createSeriesObj(seriesOptions) {
        var seriesObj = {
            id: seriesOptions.id,
            name: seriesOptions.name,
            animation: settings.animation,
            data: seriesOptions.data,
            yAxis: isNullOrUndefined(seriesOptions.yAxis) ? 0 : seriesOptions.yAxis,
            step: seriesOptions.step,
            tooltip: {
                valueDecimals: settings.decimalPlaces
            },
            marker: {
                enabled: false
            },
            lineWidth: 2,
            index: seriesOptions.index,
            showInLegend: seriesOptions.showInLegend,
            color: seriesOptions.color,
            linkedTo: seriesOptions.linkedTo,
            showInNavigator: seriesOptions.showInNavigator,
            showCustomInfo: seriesOptions.showCustomInfo
        }
        return seriesObj;
    }

    function normalize(actualValue, target) {

        var normalizedValue = null;
        target = roundFloat(target, true);
        actualValue = roundFloat(actualValue, true);
        if (!target)
            target = 1;

        if (actualValue && target)
            normalizedValue = actualValue - target;

        return normalizedValue;



    }

    //multiVarControlChart
    function getCustomTooltip(customInfo) {
        var customTooltip = [];

        if (customInfo && customInfo.filter) {
            var xAxisObj = customInfo.filter(function (k) { return k.showInTooltip = true; });
            if (xAxisObj) {
                for (var i = 0; i < xAxisObj.length; i++) {
                    var tag = tooltipRow.format(xAxisObj[i].label, xAxisObj[i].value, 'customInfo');
                    var text = tooltipRow.replace("{2}", 'customInfo');
                    text = text.replace("{0}", xAxisObj[i].label);
                    text = text.replace("{1}", xAxisObj[i].value);
                    customTooltip.push(text);
                }
            }
        }
        // return customTooltip.join('<br>');
        return customTooltip.join('');
    }
    function displayTooltip(/*tooltip*/) {
        //multiVarControlChart displayTooltip
        var msg = [];  

        msg.push(tooltipTableStart);

        // msg.push('<b>' + this.series.name + ' : ' + roundFloat(this.y, settings.decimalPlaces) + '</b></span>');
        msg.push(tooltipRow.format('<b>' + this.series.name + ' </b>', '<b> &nbsp;' + roundFloat(this.y, settings.decimalPlaces) + '</b>'));

        if (this.series.options.showCustomInfo) {
            msg.push(tooltipHr);
            msg.push(getCustomTooltip(findCustomInfo(this.series.options.id, this.point.index)));
        }
        //if ((this.key.toString().startsWith('violation') || this.key.toString().startsWith('outlier') || this.key.toString().startsWith('eliminated')) && this.point.violations) {
        //    // msg.push('<hr style="margin-top: 7px; padding: 0; margin-bottom: -10px; "/>');
        //    msg.push(tooltipHr);
        //    this.point.violations.forEach(function (v) { msg.push(tooltipRow.format(v, "")); });
        //} else if (this.series.name === seriesIds.controlChart.flagSeries && this.point) {
        //    // msg = [this.point.text]; // just show annotation
        //    msg = [];
        //    msg.push(getCustomRemarks(settings, this.point.x, true));
        //    // else return tooltip.defaultFormatter.call(this, tooltip);
        //}

        //if (this.point.text) // for flags type series, only it should be shown
        //    return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + this.point.text + "</span></div>";

        msg.push(tooltipTableEnd);

        var html = '<div style="padding:8px 16px"><span style="font-size:9pt;font-weight:400">' + msg.join('') + '</span></div>';
        //return tooltip.defaultFormatter.call(this, tooltip);
        return html;
    }

    function findCustomInfo(variableId, subgroupId) {
        var myCustomInfo;
        var myTool;
        if (variableId && subgroupId && settings.data && settings.data.variablesResponse && settings.data.variablesResponse.length > 0) {
            myTool = findActualIdFromMultiVar(variableId, settings.data.variablesResponse);
            myCustomInfo = myTool.subgroups[subgroupId].customInfo;
        }
        return myCustomInfo;
    }
    function findActualIdFromMultiVar(variableId, variableArray) {
        var myId;
        var idLength = 0;
        for (var j = 0; j < variableArray.length; j++) {
            myId = variableArray[j].variableId;
            idLength = myId.length;
            if (myId == variableId.substr(variableId.length - idLength)) {
                return variableArray[j];
            }

        }
    }

}
/**
 * @description Provide methods for multi var single value chart.
 * @namespace MultiVarSingleValueChart
 */

/**
 * @description Draw a single Value chart with multiple variable lines using the provided data from the options parameter
 * @memberof MultiVarSingleValueChart
 * @function multiVarSingleValueChart
 * @requires highstock.js
 * @requires jQuery.js
 * @param {MultiVarSingleValueChart.options} options - The multi var single Value chart options object.
 * @example
 * $('#msvc').multiVarSingleValueChart(
 *      {
 *          locale: "en",
 *          isSingleSpec: true,  //if all of the multiple variables have the same specifiation e.g. diff. tools on the same machiene
 *          chartTitle: "MultiVarSingleValueChart",
 *          messages,
 *          data: {
 *             variablesResponse: [
 *               {
 *                  variableId: "ToolNo 1",
 *                  measurements: [],
 *                  specifications: {
 *                      "SubgroupSize": 2,
 *				        "DistributionType": "Normal",
 *				        "ControlChartType": "mxb_ms",
 *				        "LimitationType": "TwoSided",
 *				        "EvaluationType": "Bosch",
 *		        	    "CapabilityStudyType": "ShortTerm",
 *				        "Limit": 0.0,
 *				        "CurrentNominalValue": 27.5,
 *				        "CurrentUpperToleranceLimitAbs": 35,
 *				        "CurrentLowerToleranceLimitAbs": 20,
 *				        "ConfidenceInterval": 4,
 *				        "ControlLimits": {},
 *				        "MovingMeanWeighting": "Uniformly",
 *				        "WeightingFactor": 0.1,
 *				        "AverageMovingMeanType": "NoAMM"
 *                  },
 *               },
 *               {
 *                  variableId: "ToolNo 2",
 *                  measurements: [],
 *                  specifications: {
 *                      "SubgroupSize": 2,
 *				        "DistributionType": "Normal",
 *				        "ControlChartType": "mxb_ms",
 *				        "LimitationType": "TwoSided",
 *				        "EvaluationType": "Bosch",
 *		        	    "CapabilityStudyType": "ShortTerm",
 *				        "Limit": 0.0,
 *				        "CurrentNominalValue": 27.5,
 *				        "CurrentUpperToleranceLimitAbs": 35,
 *				        "CurrentLowerToleranceLimitAbs": 20,
 *				        "ConfidenceInterval": 4,
 *				        "ControlLimits": {},
 *				        "MovingMeanWeighting": "Uniformly",
 *				        "WeightingFactor": 0.1,
 *				        "AverageMovingMeanType": "NoAMM"
 *                  },
 *               }
 *             ]  
 *          },
 *           decimalPlaces: 4,
 *           hiddenSeries: [
 *              {  
 *                  "seriesId": "utl_1",
 *                  "showInChart": true,
 *                  "showInLegend": true
 *               },
 *              {
 *                  "seriesId": "ltl_1",
 *                  "showInLegend": true,
 *                  "showInChart": true
 *               },
 *               {
 *                  "seriesId": "ucl_1",
 *                  "showInLegend": true,
 *                  "showInChart": true
 *               },
 *               {
 *                  "seriesId": "lcl_1",
 *                  "showInLegend": true,
 *                  "showInChart": true
 *               },
 *               {
 *                  "seriesId": "nominalValue",
 *                  "showInLegend": true,
 *                  "showInChart": false
 *               }
 *           ],
 *           onSeriesClick: function (data) {
 *                      console.log(data);
 *           },
 *           onLegendClick : function (data) {
 *                      console.log(data);
 *           }
 *        });
 */

/**
 * @description object with all neccesarry information for the chart
 * @memberof MultiVarSingleValueChart
 * @typedef {object} options
 * @property {MultiVarSingleValueChart.data} data  Array of variables up to n Variables each variable contains subgroups measurements and all other info
 * @property {string} [options.locale=en]  User locale
 * @property {string} options.chartTitle The chart's main title.
 * @property {number} [options.decimalPlaces = 4] - The number of digits to appear after the decimal point.
 * @property {number} [options.height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number} [options.width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string} [options.xAxisTitle=null] - The x-Axis label displayed underneath the x-axis.
 * @property {boolean}  [animation=false] -Enable or disable the initial animation
 * @property {boolean}  [isSingleSpec=false] -if all variables in the variableResponse - object has the same specifications must set to true, otherwise it must be false
 * @property {MultiVarSingleValueChart.seriesVisibilityObject} [hiddenSeries] - An array of series containing the IDs, visibility in the legend and the initial visibility state of the series.
 * @property {MultiVarSingleValueChart.legendSettings} legendSettings - Settings for position, layout and visibility of the chart legend
 * @property {string} xAxisTitle title of the xAxix
 * @property {MultiVarSingleValueChart.navigator} [options.navigator]  - The navigator object is to define start and end of navigator range as well as the number of selected values
 * @property {MultiVarSingleValueChart.fontSize} [fontSize]-Option to set Fontsize of label & title.
 */


/**
 * @description the container for the several varibles with measurements, subgroups, messages and specifications
 * @memberof MultiVarSingleValueChart
 * @typedef {object} data
 * @property {MultiVarSingleValueChart.variableResponse} options.data - JSON Array with Variables , specifications
 *
 * // Note: The count of measurements array objects should be based on following formula
 * // if chart type is  'x_ms', 'x_mr', 'mx_ms', 'mx_mr' then size of measurements and size of subgroups array should be same.
 * // in other case the size of measurements array should be counted like this
 * // count of measurement array objects =  subgroupSize * count of subgroup array
 */

/**
 * @description an array of different variables to show in the chart
 * @typedef {object} variableResponse
 * @memberof MultiVarSingleValueChart
 * @property {string} variableId  the name or id of the variable - e.g. 'variableId = ToolNo 1'. Every variables must have unique names/Id´s.
 * @property {Array} subgroups - The array of objects in defined format. Please look the format in the in example below.
 * @property {Array} measurements - The array of objects in defined format.  Please look the format in the in example below.
 * @property {object} specifications - The object represent the corresponding specifications e.g. subgroupSize, controlChartType
 * @property {number} specifications.subgroupSize - The size of the subgroup.
 */

/**
 * @description the behavior of series, whether to hide from legend and / or initial visible.
 * @typedef {object} seriesVisibilityObject
 * @memberof MultiVarSingleValueChart
 * @property {string} [seriesId] - the seriesId to manipulate
 * @property {boolean} [showInChart] - initial show this series or not
 * @property {boolean} [showInLegend] - display this series in the legend
 */


/**
 * @description The navigator object is to define start and end of navigator range as well as the number of selected values
 * @typedef {object} navigator
 * @memberof ControlChart
 * @property {number} [start = undefined] - first value to be selected in the navigator when defined alone then the last n values would be selected.
 * @property {number} [end = undefined] - last value to be selected in the navigator when defined alone then the first n values would be selected
 * @property {number} [selected = 25] - the number of selected value in the navigator, if start and end are set this value has no effect.
 * @example
 * navigator: {
 *               start: 0,
 *              end: 5,
 *               selected: 5
 *           },
 */


/**
 * @description The titleSettings object is to determine the position and rotation of the chart title
 * @typedef {object} titleSettings
 * @memberof MultiVarSingleValueChart
 * @property {string} [align = undefined] - The horizontal alignment of the title. Can be one of "left", "center" and "right".
 * @property {number} [rotate = undefined] - The rotation of the text in degrees. 0 is horizontal, 270 is vertical reading from bottom to top.
 * @property {number} [x = undefind] - The x position of the title relative to the alignment
 * @property {number} [y = undefind] - The y position of the title relative to the alignment
 * @example
 *titleSettings:{
 *  align: 'left',
 *  rotate: '90',
 *  x: '35',
 *  y:'207'
 *},
 */

/**
 * @description the values to set the legend to the preferred position or to hide it totally
 * @typedef {object} legendSettings
 * @memberof MultiVarSingleValueChart
 * @property {boolean} [enabled = true] - show or hide the legend.
 * @property {string} [align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */
/**
 * @description The yAxisScale object is to determine the min/max value of the yAxis if the user provide values for Min and Max then MinScaling and MaxScaling does not affect the scaling either the user specifies numeric values for min and mix or specifies the scaling behavior according to {@link scaling}
 * @typedef {object} yAxisScale
 * @memberof MultiVarSingleValueChart
 * @property {number} [min = undefined] - The minimum value of the axis. If null or undefined the minimum value is calculated based on {@link scaling}.
 * @property {number} [max = undefined] - The maximum value of the axis. If null or undefined the maximum value is calculated based on {@link scaling}.
 * @property {scaling} [minScaling = scaling.default] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {scaling} [maxScaling = scaling.default] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 */

/**
 * @description An array of objects with following properties can be injected against each measurements.
 * @typedef {object} customInfo
 * @memberof MultiVarSingleValueChart
 * @property {string} label - The label to be display in tool tip .
 * @property {string} value - The value to be used to show in tool tip  and for xAxis labels.
 * @property {boolean} showInTooltip - if true, the label and value will also be shown in tool tip for a particular subgroup as 'label: value'
 * @property {boolean} xAxisLabel - If true then value of will be shown on xAxis labels instead of default labels. e.g. subgroup number
 * @property {boolean} isRemark - If this flag is set then the value will be shown when in <b>measurement</b> (input object to charts) contains <b>statuses</b> property with category </b>Remark</b>
 * @example
 * <caption>A customInfo can be injected like this, if a data is defined in the given {@link MultiVarSingleValueChart.data.variableResponse}</caption>

    data.variableResponse[0].measurements[0].customInfo = [
        {
            label: 'Charge Number',
            value: 'CH001',
            showInTooltip: true,
            xAxisLabel: false
        },
        {
            label: 'Date',
            value: '05-01-2020',
            showInTooltip: true,
            xAxisLabel: true,
            isRemark: true
        }
    ];
 */
/**
 * @typedef {object} fontSize
 * @memberof MultiVarSingleValueChart
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */


$.fn.multiVarSingleValueChart = function (options) {
    var settings = {},
        tooltipRow = '<tr series_id="{2}"><td style="text-align: left; padding: 0;">{0}</td> <td style="text-align: right; padding: 0;">{1}</td></tr>',
        tooltipHr = '<tr><td colspan="2;" style = "padding: 3px 4px 3px 4px;"><hr style="margin-top: 0; padding: 0; margin-bottom: 0;"/></td></tr>',
        tooltipTableStart = '<table style="border-spacing: 0px;"><tbody>',
        tooltipTableEnd = '</tbody></table>';

    if (options && typeof options === 'object') {   // if not a method call
        var _options = setDefaults(options);

        settings = $.extend({
            chartType: chartTypes.svc,
            locale: 'en',
            data: {}, // JSON Array with subgroup
            chartTitle: getLocalizedText('svc_chart', _options.locale),
            decimalPlaces: 4,
            xAxisTitle: undefined,
            yAxisTitle: undefined,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            navigator: {
                start: undefined,
                end: undefined,
                selected: 25
            },
            onSeriesClick: undefined,
            yAxisScale: {
                min: undefined,
                max: undefined,
                minScaling: scaling.default,
                maxScaling: scaling.default
            },
            onLegendClick: undefined,
            onChartLoaded: undefined,
            yAxisUnit: getDefaultyAxisUnitObj(),
            hiddenSeries: [],
            siemensTooltip: {},
            legendSettings: {
                enabled: undefined,
                align: undefined,
                verticalAlign: undefined,
                layout: undefined,
                x: undefined,
                y: undefined
            },
            titleSettings: undefined,
            animation: false,
            series: getDefaultSeriesObj()
        }, _options, { chartType: chartTypes.svc });
    } else if (typeof options === 'string') { // Method call
        settings = $(this).data('settings') || {};  // restore the settings object from prev settings
    }
    var dataExistsComplete = false;
    if (settings.data && settings.data.variablesResponse && settings.data.variablesResponse[0].measurements && settings.data.variablesResponse[0].specifications) {
        dataExistsComplete = true;
    }
    var $this = this,
        _args = arguments,
        chart = {
            init: function () {
                var containerId = $this.attr('id'),
                    input = [];
                for (var i = 0; i < settings.data.variablesResponse.length; i++) {
                    var variable = {
                        variableI: settings.data.variablesResponse[i].variableId, data: settings.data.variablesResponse[i],
                        series: [], ucl: [], lcl: [],
                        uwl: [], lwl: [], xbp: [],
                        utl: [], ltl: [],
                        sortNumbers: [],
                        customInfo: [], sequenceIds: [],
                        flagSeries: [], nominalValue: [],
                        firstChoiceUpperLimit: [], firstChoiceLowerLimit: [], secondChoiceUpperLimit: [], secondChoiceLowerLimit: [], thirdChoiceUpperLimit: [], thirdChoiceLowerLimit: [],
                        centerOfTolerances: [], averageOfAllValues: [], oneThirdOfUpperControlLimit: [], twoThirdsOfUpperControlLimit: [], oneThirdOfLowerControlLimit: [], twoThirdsOfLowerControlLimit: []
                    };
                    input.push(variable);
                    prepareData(input[i]);
                }


                if (dataExistsComplete) {
                    settings.inputs = input;    // for tooltip etc.

                    var _chart = Highcharts.stockChart(containerId, prepareChartOptions(input));
                    //  chart.xAxis[0].setExtremes(75, 100);	// Initial select last 25 samples
                    $this.data('chartApi', _chart);
                    // $this.data('inputData', input); // to get the series data on method call
                    $this.data('settings', settings);

                } else
                    throw 'Invalid data format exception: Expected data is not in defined format. Please consult developer documentation.';
            }
        };
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2], _args[3], _args[4]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function prepareData(input) {
        var hasSubgroupAdditionalLines = false;
        if (input.data && input.data.additionalLines) {
            hasSubgroupAdditionalLines = true;
        };

        $.each(input.data.measurements, function (i, p) {
            var data = input.data,
                subgrpNum = isSubequalMeasure(data.specifications.controlChartType) ? i : p.subgroupNumber,
                statuses = p.statuses ? getVoilations(p.statuses) : [],
                val = roundFloat(p.measuredValue),
                subgroup = data.subgroups[subgrpNum] || {},
                customInfo = p.customInfo || [], // May be not each subgroup contain customInfo, therefore empty object should be used for corresponding subgroup
                toolChanged_status = getMeasurmentStatuses(p, statusesCategory.toolchanged) || [],
                remark_status = getMeasurmentStatuses(p, statusesCategory.remark) || [],
                attachment_status = getMeasurmentStatuses(p, statusesCategory.attachment) || [],
                ucl = roundFloat(subgroup.upperControlLimit1Abs, null, true),
                lcl = roundFloat(subgroup.lowerControlLimit1Abs, null, true),
                uwl = roundFloat(subgroup.upperWarnLimit1Abs, null, true),
                lwl = roundFloat(subgroup.lowerWarnLimit1Abs, null, true),
                xbp = roundFloat(subgroup.calculatedProcessMeanValue1, null, true),
                utl = roundFloat(subgroup.upperToleranceLimitAbs, null, true),
                ltl = roundFloat(subgroup.lowerToleranceLimitAbs, null, true),
                nominalValue = roundFloat(subgroup.nominalValue, null, true);

            if (p.upperToleranceLimitAbs != null) {
                utl = roundFloat(p.upperToleranceLimitAbs, null, true)
            }
            if (p.lowerToleranceLimitAbs != null) {
                ltl = roundFloat(p.lowerToleranceLimitAbs, null, true)
            }
            if (p.nominalValue != null) {
                nominalValue = roundFloat(p.nominalValue, null, true)
            }
            if (nominalValue == null) {
                nominalValue = data.specifications.currentNominalValue;
            }

            if (input.data && input.data.additionalLines)
                addAdditionalLines(input, subgroup, hasSubgroupAdditionalLines);

            if (statuses.indexOf('processviolation_1') > -1 || statuses.indexOf('processviolation_2') > -1)
                input.series.push({
                    marker: {
                        fillColor: chartSeriesColors.SingleValueChart.Marker,
                        lineWidth: 3,
                        lineColor: chartSeriesColors.SingleValueChart.Marker,
                        symbol: 'triangle'
                    },
                    name: 'violation' + i,
                    y: val,
                    vType: statuses
                });
            else if (statuses.indexOf('outlier') > -1) {
                input.series.push({
                    marker: {
                        fillColor: chartSeriesColors.SingleValueChart.YellowMarker,
                        lineWidth: 3,
                        lineColor: chartSeriesColors.SingleValueChart.YellowMarker,
                        symbol: 'triangle'
                    },
                    name: 'outlier' + i,
                    y: val,
                    vType: statuses
                });
            } else if (statuses.indexOf("eliminated") > -1) {
                input.series.push({
                    marker: {
                        fillColor: chartSeriesColors.SingleValueChart.Marker,
                        lineWidth: 3,
                        lineColor: chartSeriesColors.SingleValueChart.Marker,
                        symbol: 'cross'
                    },
                    name: 'eliminated',
                    y: val,
                    vType: statuses
                })
            } else if (statuses.indexOf("eliminatedbycalculation") > -1) {
                input.series.push({
                    marker: {
                        fillColor: chartSeriesColors.SingleValueChart.Marker,
                        lineWidth: 3,
                        lineColor: chartSeriesColors.SingleValueChart.Marker,
                        symbol: 'cross'
                    },
                    name: 'eliminatedbycalculation',
                    y: val,
                    vType: statuses
                });
            }
            else
                input.series.push(val);

            if (settings.xAxisCustomInfoLabel === true) {
                if (customInfo) {
                    input.sortNumbers.push(getCustomxAxisLabel(customInfo));
                } else { input.sortNumbers.push(null); }
            } else
                input.sortNumbers.push(roundFloat(p.sequenceID));

            input.sequenceIds.push(roundFloat(p.sequenceID)); //tooltip

            input.ucl.push(ucl);
            input.lcl.push(lcl);
            input.uwl.push(uwl);
            input.lwl.push(lwl);
            input.xbp.push(xbp);
            input.utl.push(utl);
            input.ltl.push(ltl);
            input.nominalValue.push(nominalValue);
            p.violations = subgroup.statuses;

            if (p.subgroupNumber == null) {
                input.ucl[i] = input.ucl[i - 1];
                input.lcl[i] = input.lcl[i - 1];
                input.uwl[i] = input.uwl[i - 1];
                input.lwl[i] = input.lwl[i - 1];
                input.xbp[i] = input.xbp[i - 1];
                input.utl[i] = input.utl[i - 1];
                input.ltl[i] = input.ltl[i - 1];
                input.nominalValue[i] = input.nominalValue[i - 1];
            }

            if (toolChanged_status.length > 0) {

                input.flagSeries.push({
                    y: val,
                    x: i,
                    title: svgRepository.ToolChanged,
                    text: getLocalizedText('ToolChanged', settings.locale),
                });
            }
            if (remark_status.length > 0) {
                input.flagSeries.push({
                    y: val,
                    x: i,
                    title: svgRepository.Remark,
                    text: getCustomRemarks(settings, i)
                });
            }
            if (attachment_status.length > 0) {
                input.flagSeries.push({
                    y: val,
                    x: i,
                    title: svgRepository.Attachment,
                    text: getLocalizedText('Attachment', settings.locale),

                });
            }

            input.customInfo.push(customInfo);
        });
    }

    function prepareSeries(input) {
        var ser = [];
        var colors = Object.values(chartSeriesColors.multiVarControlChart);
        var colorIndex = 0;

        for (var i = 0; i < input.length; i++) {
            ser.push(
                {
                    id: seriesIds.singleValueChart.measurement + " " + input[i].variableI,
                    name: input[i].variableI,
                    animation: settings.animation,
                    data: input[i].series,
                    yAxis: 0,
                    color: colors[colorIndex],
                    tooltip: {
                        valueDecimals: settings.decimalPlaces
                    },
                    marker: {
                        enabled: false,
                        symbol: 'circle',
                        radius: 4
                    },
                    legendIndex: 1,
                    showInLegend: true,
                    visible: true,
                    showCustomInfo: true,
                    events: {
                        click: onSeriesClick
                    },
                    zoneAxis: 'x',
                    zones: prepareColoredZone(i)
                }
            );
            colorIndex++;
            if (colorIndex > 19) colorIndex = 0;


            if (settings.isSingleSpec) {
                if (i == 0) {
                    ser.push(
                        {
                            id: seriesIds.singleValueChart.utl + " " + input[i].variableI,
                            name: getLocalizedText('UTL_1', settings.locale), //  Highcharts.uiLocale[settings.locale].UTL_1,
                            animation: settings.animation,
                            data: input[i].utl,
                            yAxis: 0,
                            color: chartSeriesColors.SingleValueChart.ToleranceLimit,
                            step: 'left',
                            tooltip: {
                                valueDecimals: settings.decimalPlaces
                            },
                            marker: {
                                enabled: false
                            },
                            lineWidth: 1.2,
                            legendIndex: 2,
                            showInLegend: seriesVisibility(seriesIds.singleValueChart.utl, input[i].utl, settings.hiddenSeries, true).showInLegend,
                            visible: seriesVisibility(seriesIds.singleValueChart.utl, input[i].utl, settings.hiddenSeries, true).showInChart,
                            events: {
                                click: onSeriesClick
                            }
                        }
                    );
                    ser.push(
                        {
                            id: seriesIds.singleValueChart.ucl + " " + input[i].variableI,
                            name: getLocalizedText('UCL_1', settings.locale), //  Highcharts.uiLocale[settings.locale].UCL_1,
                            animation: settings.animation,
                            data: input[i].ucl,
                            yAxis: 0,
                            color: chartSeriesColors.SingleValueChart.ControlLimit,
                            step: 'left',
                            tooltip: {
                                valueDecimals: settings.decimalPlaces
                            },
                            marker: {
                                enabled: false
                            },
                            lineWidth: 1.2,
                            legendIndex: 3,
                            showInLegend: seriesVisibility(seriesIds.singleValueChart.ucl, input[i].ucl, settings.hiddenSeries, true).showInLegend,
                            visible: seriesVisibility(seriesIds.singleValueChart.ucl, input[i].ucl, settings.hiddenSeries, true).showInChart,
                            events: {
                                click: onSeriesClick
                            }
                        }
                    );
                    ser.push(
                        {
                            id: seriesIds.singleValueChart.uwl + " " + input[i].variableI,
                            name: getLocalizedText('UWL_1', settings.locale),
                            animation: settings.animation,
                            data: input[i].uwl,
                            yAxis: 0,
                            color: chartSeriesColors.SingleValueChart.WarningLimit,
                            step: 'left',
                            tooltip: {
                                valueDecimals: settings.decimalPlaces
                            },
                            marker: {
                                enabled: false
                            },
                            lineWidth: 1.2,
                            legendIndex: 4,
                            showInLegend: seriesVisibility(seriesIds.singleValueChart.uwl, input[i].uwl, settings.hiddenSeries, true).showInLegend,
                            visible: seriesVisibility(seriesIds.singleValueChart.uwl, input[i].uwl, settings.hiddenSeries, true).showInChart,
                            events: {
                                click: onSeriesClick
                            }
                        }
                    );
                    ser.push(
                        {
                            id: seriesIds.singleValueChart.xbp + " " + input[i].variableI,
                            name: getLocalizedText('xbp_1', settings.locale), //  Highcharts.uiLocale[settings.locale].xbp_1,
                            animation: settings.animation,
                            data: input[i].xbp,
                            yAxis: 0,
                            color: chartSeriesColors.SingleValueChart.xbp,
                            step: 'left',
                            tooltip: {
                                valueDecimals: settings.decimalPlaces
                            },
                            marker: {
                                enabled: false
                            },
                            lineWidth: 1.2,
                            legendIndex: 5,
                            //showInLegend: seriesVisibility(seriesIds.singleValueChart.xbp, input[i].xbp, settings.hiddenSeries, true).showInLegend,
                            //visible: seriesVisibility(seriesIds.singleValueChart.xbp, input[i].xbp, settings.hiddenSeries, true).showInChart,
                            showInLegend: false,
                            visible: false,
                            events: {
                                click: onSeriesClick
                            }
                        }
                    );
                    ser.push(
                        {
                            id: seriesIds.singleValueChart.lwl + " " + input[i].variableI,
                            name: getLocalizedText('LWL_1', settings.locale), //  Highcharts.uiLocale[settings.locale].LWL_1,
                            animation: settings.animation,
                            data: input[i].lwl,
                            yAxis: 0,
                            color: chartSeriesColors.SingleValueChart.WarningLimit,
                            step: 'left',
                            tooltip: {
                                valueDecimals: settings.decimalPlaces
                            },
                            marker: {
                                enabled: false
                            },
                            lineWidth: 1.2,
                            legendIndex: 6,
                            showInLegend: seriesVisibility(seriesIds.singleValueChart.lwl, input[i].lwl, settings.hiddenSeries, true).showInLegend,
                            visible: seriesVisibility(seriesIds.singleValueChart.lwl, input[i].lwl, settings.hiddenSeries, true).showInChart,
                            events: {
                                click: onSeriesClick
                            }
                        }
                    );
                    ser.push(
                        {
                            id: seriesIds.singleValueChart.lcl + " " + input[i].variableI,
                            name: getLocalizedText('LCL_1', settings.locale), //  Highcharts.uiLocale[settings.locale].LCL_1,
                            animation: settings.animation,
                            data: input[i].lcl,
                            yAxis: 0,
                            color: chartSeriesColors.SingleValueChart.ControlLimit,
                            step: 'left',
                            tooltip: {
                                valueDecimals: settings.decimalPlaces
                            },
                            marker: {
                                enabled: false
                            },
                            lineWidth: 1.2,
                            legendIndex: 7,
                            showInLegend: seriesVisibility(seriesIds.singleValueChart.lcl, input[i].lcl, settings.hiddenSeries, true).showInLegend,
                            visible: seriesVisibility(seriesIds.singleValueChart.lcl, input[i].lcl, settings.hiddenSeries, true).showInChart,
                            events: {
                                click: onSeriesClick
                            }
                        }
                    );
                    ser.push(
                        {
                            id: seriesIds.singleValueChart.ltl + " " + input[i].variableI,
                            name: getLocalizedText('LTL_1', settings.locale), //  Highcharts.uiLocale[settings.locale].LTL_1,
                            animation: settings.animation,
                            data: input[i].ltl,
                            yAxis: 0,
                            color: chartSeriesColors.SingleValueChart.ToleranceLimit,
                            step: 'left',
                            tooltip: {
                                valueDecimals: settings.decimalPlaces
                            },
                            marker: {
                                enabled: false
                            },
                            lineWidth: 1.2,
                            legendIndex: 8,
                            showInLegend: seriesVisibility(seriesIds.singleValueChart.ltl, input[i].ltl, settings.hiddenSeries, true).showInLegend,
                            visible: seriesVisibility(seriesIds.singleValueChart.ltl, input[i].ltl, settings.hiddenSeries, true).showInChart,
                            events: {
                                click: onSeriesClick
                            }
                        }
                    );
                    ser.push(
                        {
                            id: seriesIds.singleValueChart.flagSeries + " " + input[i].variableI,
                            name: seriesIds.singleValueChart.flagSeries,
                            animation: settings.animation,
                            type: 'flags',
                            data: input[i].flagSeries,
                            onSeries: seriesIds.singleValueChart.measurement,
                            showInLegend: false,
                            useHTML: true,
                            dataLabels: {
                                useHTML: true,
                            },
                            lineWidth: 1,
                            lineColor: '#005F87',
                            stackDistance: 10
                        }
                    );
                    if (input[i].data.additionalLines) {
                        ser.push(
                            {
                                id: seriesIds.controlChart.centerOfTolerances + " " + input[i].variableI,
                                name: getLocalizedText('centerOfTolerances', settings.locale),
                                animation: settings.animation,
                                data: input[i].centerOfTolerances,
                                yAxis: 0,
                                color: chartSeriesColors.SingleValueChart.nominalValue.CenterOfTolerances,
                                step: 'left',
                                tooltip: {
                                    valueDecimals: settings.decimalPlaces
                                },
                                marker: {
                                    enabled: false
                                },
                                lineWidth: 1.2,
                                legendIndex: 16,
                                showInLegend: seriesVisibility(seriesIds.singleValueChart.centerOfTolerances, input[i].centerOfTolerances, settings.hiddenSeries, false).showInLegend,
                                visible: seriesVisibility(seriesIds.singleValueChart.centerOfTolerances, input[i].centerOfTolerances, settings.hiddenSeries, false).showInChart,
                                events: {
                                    click: onSeriesClick
                                }
                            }
                        );
                    };
                    ser.push(
                        {
                            id: seriesIds.singleValueChart.nominalValue + " " + input[i].variableI,
                            name: getLocalizedText('nominalValue', settings.locale), //  Highcharts.uiLocale[settings.locale].nominalValue,
                            animation: settings.animation,
                            data: input[i].nominalValue,
                            yAxis: 0,
                            color: chartSeriesColors.SingleValueChart.nominalValue,
                            step: 'left',
                            tooltip: {
                                valueDecimals: settings.decimalPlaces
                            },
                            marker: {
                                enabled: false
                            },
                            lineWidth: 1.2,
                            legendIndex: 22,
                            showInLegend: seriesVisibility(seriesIds.singleValueChart.nominalValue, input[i].nominalValue, settings.hiddenSeries, true).showInLegend,
                            visible: seriesVisibility(seriesIds.singleValueChart.nominalValue, input[i].nominalValue, settings.hiddenSeries, true).showInChart,
                            events: {
                                click: onSeriesClick
                            }
                        }
                    );
                        

                    //==============================
                    //  first, second and third choise (for future use)
                    //==============================
                    if (false) {

                        //ser.push(
                        //    {
                        //        id: seriesIds.singleValueChart.firstChoiceUpperLimit + " " + input[i].variableI,
                        //        name: getLocalizedText('firstChoiceUpperLimit', settings.locale),
                        //        animation: settings.animation,
                        //        data: input[i].data.additionalLines.firstChoiceUpperLimit,
                        //        yAxis: 0,
                        //        color: chartSeriesColors.SingleValueChart.FirstChoice,
                        //        step: 'left',
                        //        tooltip: {
                        //            valueDecimals: settings.decimalPlaces
                        //        },
                        //        marker: {
                        //            enabled: false
                        //        },
                        //        lineWidth: 1.2,
                        //        legendIndex: 9,
                        //        showInLegend: seriesVisibility(seriesIds.singleValueChart.firstChoiceUpperLimit, input[i].firstChoiceUpperLimit, settings.hiddenSeries, false).showInLegend,
                        //        visible: seriesVisibility(seriesIds.singleValueChart.firstChoiceUpperLimit, input[i].firstChoiceUpperLimit, settings.hiddenSeries, false).showInChart,
                        //        events: {
                        //            click: onSeriesClick
                        //        }
                        //    }
                        //);
                        //ser.push(
                        //    {
                        //        id: seriesIds.singleValueChart.firstChoiceLowerLimit + " " + input[i].variableI,
                        //        linkedTo: seriesIds.singleValueChart.firstChoiceUpperLimit + " " + input[i].variableI,
                        //        //name: getLocalizedText('firstChoiceLowerLimit', settings.locale),
                        //        animation: settings.animation,
                        //        data: input[i].data.additionalLines.firstChoiceLowerLimit,
                        //        yAxis: 0,
                        //        color: chartSeriesColors.SingleValueChart.FirstChoice,
                        //        step: 'left',
                        //        tooltip: {
                        //            valueDecimals: settings.decimalPlaces
                        //        },
                        //        marker: {
                        //            enabled: false
                        //        },
                        //        lineWidth: 1.2,
                        //        legendIndex: 11,
                        //        events: {
                        //            click: onSeriesClick
                        //        }
                        //    }
                        //);
                        //ser.push(
                        //    {
                        //        id: seriesIds.singleValueChart.secondChoiceUpperLimit + " " + input[i].variableI,
                        //        name: getLocalizedText('secondChoiceUpperLimit', settings.locale),
                        //        animation: settings.additionalLines.animation,
                        //        data: input[i].data.additionalLines.secondChoiceUpperLimit,
                        //        yAxis: 0,
                        //        color: chartSeriesColors.SingleValueChart.SecondChoice,
                        //        step: 'left',
                        //        tooltip: {
                        //            valueDecimals: settings.decimalPlaces
                        //        },
                        //        marker: {
                        //            enabled: false
                        //        },
                        //        lineWidth: 1.2,
                        //        legendIndex: 12,
                        //        showInLegend: seriesVisibility(seriesIds.singleValueChart.secondChoiceUpperLimit, input[i].secondChoiceUpperLimit, settings.hiddenSeries, false).showInLegend,
                        //        visible: seriesVisibility(seriesIds.singleValueChart.secondChoiceUpperLimit, input[i].secondChoiceUpperLimit, settings.hiddenSeries, false).showInChart,
                        //        events: {
                        //            click: onSeriesClick
                        //        }
                        //    }
                        //);
                        //ser.push(
                        //    {
                        //        id: seriesIds.singleValueChart.secondChoiceLowerLimit + " " + input[i].variableI,
                        //        linkedTo: seriesIds.singleValueChart.secondChoiceUpperLimit + " " + input[i].variableI,
                        //        //name: getLocalizedText('secondChoiceLowerLimit', settings.locale), 
                        //        animation: settings.animation,
                        //        data: input[i].data.additionalLines.secondChoiceLowerLimit,
                        //        yAxis: 0,
                        //        color: chartSeriesColors.SingleValueChart.SecondChoice,
                        //        step: 'left',
                        //        tooltip: {
                        //            valueDecimals: settings.decimalPlaces
                        //        },
                        //        marker: {
                        //            enabled: false
                        //        },
                        //        lineWidth: 1.2,
                        //        legendIndex: 13,
                        //        events: {
                        //            click: onSeriesClick
                        //        }
                        //    }
                        //);
                        //ser.push(
                        //    {
                        //        id: seriesIds.singleValueChart.thirdChoiceUpperLimit + " " + input[i].variableI,
                        //        name: getLocalizedText('thirdChoiceUpperLimit', settings.locale),
                        //        animation: settings.animation,
                        //        data: input[i].data.additionalLines.thirdChoiceUpperLimit,
                        //        yAxis: 0,
                        //        color: chartSeriesColors.SingleValueChart.ThirdChoice,
                        //        step: 'left',
                        //        tooltip: {
                        //            valueDecimals: settings.decimalPlaces
                        //        },
                        //        marker: {
                        //            enabled: false
                        //        },
                        //        lineWidth: 1.2,
                        //        legendIndex: 14,
                        //        showInLegend: seriesVisibility(seriesIds.singleValueChart.thirdChoiceUpperLimit, input[i].thirdChoiceUpperLimit, settings.hiddenSeries, false).showInLegend,
                        //        visible: seriesVisibility(seriesIds.singleValueChart.thirdChoiceUpperLimit, input[i].thirdChoiceUpperLimit, settings.hiddenSeries, false).showInChart,
                        //        events: {
                        //            click: onSeriesClick
                        //        }
                        //    }
                        //);
                        //ser.push(
                        //    {
                        //        id: seriesIds.singleValueChart.thirdChoiceLowerLimit + " " + input[i].variableI,
                        //        linkedTo: seriesIds.singleValueChart.thirdChoiceUpperLimit + " " + input[i].variableI,
                        //        //name: getLocalizedText('thirdChoiceLowerLimit', settings.locale), 
                        //        animation: settings.animation,
                        //        data: input[i].data.additionalLines.thirdChoiceLowerLimit,
                        //        yAxis: 0,
                        //        color: chartSeriesColors.SingleValueChart.ThirdChoice,
                        //        step: 'left',
                        //        tooltip: {
                        //            valueDecimals: settings.decimalPlaces
                        //        },
                        //        marker: {
                        //            enabled: false
                        //        },
                        //        lineWidth: 1.2,
                        //        legendIndex: 15,
                        //        events: {
                        //            click: onSeriesClick
                        //        }
                        //    }
                        //);
                        

                    }

                    //==============================
                    //  more unused series (for future use)
                    //==============================
                    if (false) {
                  
                    //ser.push(
                    //    {
                    //        id: seriesIds.singleValueChart.averageOfAllValues + " " + input[i].variableI,
                    //        name: getLocalizedText('averageOfAllValues', settings.locale),
                    //        animation: settings.animation,
                    //        data: input[i].averageOfAllValues,
                    //        yAxis: 0,
                    //        color: chartSeriesColors.SingleValueChart.averageOfAllValues,
                    //        step: 'left',
                    //        tooltip: {
                    //            valueDecimals: settings.decimalPlaces
                    //        },
                    //        marker: {
                    //            enabled: false
                    //        },
                    //        lineWidth: 1.2,
                    //        legendIndex: 17,
                    //        showInLegend: seriesVisibility(seriesIds.singleValueChart.averageOfAllValues, input[i].averageOfAllValues, settings.hiddenSeries, false).showInLegend,
                    //        visible: seriesVisibility(seriesIds.singleValueChart.averageOfAllValues, input[i].averageOfAllValues, settings.hiddenSeries, false).showInChart,
                    //        events: {
                    //            click: onSeriesClick
                    //        }
                    //    }
                    //);
                    //ser.push(
                    //    {
                    //        id: seriesIds.singleValueChart.oneThirdOfUpperControlLimit + " " + input[i].variableI,
                    //        name: getLocalizedText('oneThirdOfControlLimits', settings.locale),
                    //        animation: settings.animation,
                    //        data: input[i].oneThirdOfUpperControlLimit,
                    //        yAxis: 0,
                    //        color: chartSeriesColors.SingleValueChart.oneThirdOfControlLimits,
                    //        step: 'left',
                    //        tooltip: {
                    //            valueDecimals: settings.decimalPlaces
                    //        },
                    //        marker: {
                    //            enabled: false
                    //        },
                    //        lineWidth: 1.2,
                    //        legendIndex: 18,
                    //        showInLegend: seriesVisibility(seriesIds.singleValueChart.oneThirdOfUpperControlLimit, input[i].oneThirdOfUpperControlLimit, settings.hiddenSeries, false).showInLegend,
                    //        visible: seriesVisibility(seriesIds.singleValueChart.oneThirdOfUpperControlLimit, input[i].oneThirdOfUpperControlLimit, settings.hiddenSeries, false).showInChart,
                    //        events: {
                    //            click: onSeriesClick
                    //        }
                    //    }
                    //);
                    //ser.push(
                    //    {
                    //        id: seriesIds.singleValueChart.twoThirdsOfUpperControlLimit + " " + input[i].variableI,
                    //        name: getLocalizedText('twoThirdsOfControlLimits', settings.locale),
                    //        animation: settings.animation,
                    //        data: input[i].twoThirdsOfUpperControlLimit,
                    //        yAxis: 0,
                    //        color: chartSeriesColors.SingleValueChart.twoThirdsOfControlLimits,
                    //        step: 'left',
                    //        tooltip: {
                    //            valueDecimals: settings.decimalPlaces
                    //        },
                    //        marker: {
                    //            enabled: false
                    //        },
                    //        lineWidth: 1.2,
                    //        legendIndex: 19,
                    //        showInLegend: seriesVisibility(seriesIds.singleValueChart.twoThirdsOfUpperControlLimit, input[i].twoThirdsOfUpperControlLimit, settings.hiddenSeries, false).showInLegend,
                    //        visible: seriesVisibility(seriesIds.singleValueChart.twoThirdsOfUpperControlLimit, input[i].twoThirdsOfUpperControlLimit, settings.hiddenSeries, false).showInChart,
                    //        events: {
                    //            click: onSeriesClick
                    //        }
                    //    }
                    //);
                    //ser.push(
                    //    {
                    //        id: seriesIds.singleValueChart.oneThirdOfLowerControlLimit + " " + input[i].variableI,
                    //        linkedTo: seriesIds.singleValueChart.oneThirdOfUpperControlLimit + " " + input[i].variableI,
                    //        //name: getLocalizedText('oneThirdOfLowerControlLimit', settings.locale), 
                    //        animation: settings.animation,
                    //        data: input[i].oneThirdOfLowerControlLimit,
                    //        yAxis: 0,
                    //        color: chartSeriesColors.SingleValueChart.oneThirdOfControlLimits,
                    //        step: 'left',
                    //        tooltip: {
                    //            valueDecimals: settings.decimalPlaces
                    //        },
                    //        marker: {
                    //            enabled: false
                    //        },
                    //        lineWidth: 1.2,
                    //        legendIndex: 20,
                    //        events: {
                    //            click: onSeriesClick
                    //        }
                    //    }
                    //);
                    //ser.push(
                    //    {
                    //        id: seriesIds.singleValueChart.twoThirdsOfLowerControlLimit + " " + input[i].variableI,
                    //        linkedTo: seriesIds.singleValueChart.twoThirdsOfUpperControlLimit + " " + input[i].variableI,
                    //        //name: getLocalizedText('twoThirdsOfLowerControlLimit', settings.locale),
                    //        animation: settings.animation,
                    //        data: input[i].twoThirdsOfLowerControlLimit,
                    //        yAxis: 0,
                    //        color: chartSeriesColors.SingleValueChart.twoThirdsOfControlLimits,
                    //        step: 'left',
                    //        tooltip: {
                    //            valueDecimals: settings.decimalPlaces
                    //        },
                    //        marker: {
                    //            enabled: false
                    //        },
                    //        lineWidth: 1.2,
                    //        legendIndex: 21,
                    //        events: {
                    //            click: onSeriesClick
                    //        }
                    //    }
                    //);
                    

                    }
                }
            } else {
                ser.push(
                    {
                        id: seriesIds.singleValueChart.utl + " " + input[i].variableI,
                        name: getLocalizedText('UTL_1', settings.locale) + " " + input[i].variableI, //  Highcharts.uiLocale[settings.locale].UTL_1,
                        animation: settings.animation,
                        data: input[i].utl,
                        yAxis: 0,
                        color: chartSeriesColors.SingleValueChart.ToleranceLimit,
                        step: 'left',
                        tooltip: {
                            valueDecimals: settings.decimalPlaces
                        },
                        marker: {
                            enabled: false
                        },
                        lineWidth: 1.2,
                        legendIndex: 2,
                        showInLegend: seriesVisibility(seriesIds.singleValueChart.utl, input[i].utl, settings.hiddenSeries, true).showInLegend,
                        visible: seriesVisibility(seriesIds.singleValueChart.utl, input[i].utl, settings.hiddenSeries, true).showInChart,
                        events: {
                            click: onSeriesClick
                        }
                    }
                );
                ser.push(
                    {
                        id: seriesIds.singleValueChart.ucl + " " + input[i].variableI,
                        name: getLocalizedText('UCL_1', settings.locale) + " " + input[i].variableI, //  Highcharts.uiLocale[settings.locale].UCL_1,
                        animation: settings.animation,
                        data: input[i].ucl,
                        yAxis: 0,
                        color: chartSeriesColors.SingleValueChart.ControlLimit,
                        step: 'left',
                        tooltip: {
                            valueDecimals: settings.decimalPlaces
                        },
                        marker: {
                            enabled: false
                        },
                        lineWidth: 1.2,
                        legendIndex: 3,
                        showInLegend: seriesVisibility(seriesIds.singleValueChart.ucl, input[i].ucl, settings.hiddenSeries, true).showInLegend,
                        visible: seriesVisibility(seriesIds.singleValueChart.ucl, input[i].ucl, settings.hiddenSeries, true).showInChart,
                        events: {
                            click: onSeriesClick
                        }
                    }
                );
                ser.push(
                    {
                        id: seriesIds.singleValueChart.uwl + " " + input[i].variableI,
                        name: getLocalizedText('UWL_1', settings.locale) + " " + input[i].variableI,
                        animation: settings.animation,
                        data: input[i].uwl,
                        yAxis: 0,
                        color: chartSeriesColors.SingleValueChart.WarningLimit,
                        step: 'left',
                        tooltip: {
                            valueDecimals: settings.decimalPlaces
                        },
                        marker: {
                            enabled: false
                        },
                        lineWidth: 1.2,
                        legendIndex: 4,
                        showInLegend: seriesVisibility(seriesIds.singleValueChart.uwl, input[i].uwl, settings.hiddenSeries, true).showInLegend,
                        visible: seriesVisibility(seriesIds.singleValueChart.uwl, input[i].uwl, settings.hiddenSeries, true).showInChart,
                        events: {
                            click: onSeriesClick
                        }
                    }
                );
                ser.push(
                    {
                        id: seriesIds.singleValueChart.xbp + " " + input[i].variableI,
                        name: getLocalizedText('xbp_1', settings.locale) + " " + input[i].variableI, //  Highcharts.uiLocale[settings.locale].xbp_1,
                        animation: settings.animation,
                        data: input[i].xbp,
                        yAxis: 0,
                        color: chartSeriesColors.SingleValueChart.xbp,
                        step: 'left',
                        tooltip: {
                            valueDecimals: settings.decimalPlaces
                        },
                        marker: {
                            enabled: false
                        },
                        lineWidth: 1.2,
                        legendIndex: 5,
                        showInLegend: seriesVisibility(seriesIds.singleValueChart.xbp, input[i].xbp, settings.hiddenSeries, true).showInLegend,
                        visible: seriesVisibility(seriesIds.singleValueChart.xbp, input[i].xbp, settings.hiddenSeries, true).showInChart,
                        events: {
                            click: onSeriesClick
                        }
                    }
                );
                ser.push(
                    {
                        id: seriesIds.singleValueChart.lwl + " " + input[i].variableI,
                        name: getLocalizedText('LWL_1', settings.locale) + " " + input[i].variableI, //  Highcharts.uiLocale[settings.locale].LWL_1,
                        animation: settings.animation,
                        data: input[i].lwl,
                        yAxis: 0,
                        color: chartSeriesColors.SingleValueChart.WarningLimit,
                        step: 'left',
                        tooltip: {
                            valueDecimals: settings.decimalPlaces
                        },
                        marker: {
                            enabled: false
                        },
                        lineWidth: 1.2,
                        legendIndex: 6,
                        showInLegend: seriesVisibility(seriesIds.singleValueChart.lwl, input[i].lwl, settings.hiddenSeries, true).showInLegend,
                        visible: seriesVisibility(seriesIds.singleValueChart.lwl, input[i].lwl, settings.hiddenSeries, true).showInChart,
                        events: {
                            click: onSeriesClick
                        }
                    }
                );
                ser.push(
                    {
                        id: seriesIds.singleValueChart.lcl + " " + input[i].variableI,
                        name: getLocalizedText('LCL_1', settings.locale) + " " + input[i].variableI, //  Highcharts.uiLocale[settings.locale].LCL_1,
                        animation: settings.animation,
                        data: input[i].lcl,
                        yAxis: 0,
                        color: chartSeriesColors.SingleValueChart.ControlLimit,
                        step: 'left',
                        tooltip: {
                            valueDecimals: settings.decimalPlaces
                        },
                        marker: {
                            enabled: false
                        },
                        lineWidth: 1.2,
                        legendIndex: 7,
                        showInLegend: seriesVisibility(seriesIds.singleValueChart.lcl, input[i].lcl, settings.hiddenSeries, true).showInLegend,
                        visible: seriesVisibility(seriesIds.singleValueChart.lcl, input[i].lcl, settings.hiddenSeries, true).showInChart,
                        events: {
                            click: onSeriesClick
                        }
                    }
                );
                ser.push(
                    {
                        id: seriesIds.singleValueChart.ltl + " " + input[i].variableI,
                        name: getLocalizedText('LTL_1', settings.locale) + " " + input[i].variableI, //  Highcharts.uiLocale[settings.locale].LTL_1,
                        animation: settings.animation,
                        data: input[i].ltl,
                        yAxis: 0,
                        color: chartSeriesColors.SingleValueChart.ToleranceLimit,
                        step: 'left',
                        tooltip: {
                            valueDecimals: settings.decimalPlaces
                        },
                        marker: {
                            enabled: false
                        },
                        lineWidth: 1.2,
                        legendIndex: 8,
                        showInLegend: seriesVisibility(seriesIds.singleValueChart.ltl, input[i].ltl, settings.hiddenSeries, true).showInLegend,
                        visible: seriesVisibility(seriesIds.singleValueChart.ltl, input[i].ltl, settings.hiddenSeries, true).showInChart,
                        events: {
                            click: onSeriesClick
                        }
                    }
                );
                ser.push(
                    {
                        id: seriesIds.singleValueChart.flagSeries + " " + input[i].variableI,
                        name: seriesIds.singleValueChart.flagSeries,
                        animation: settings.animation,
                        type: 'flags',
                        data: input[i].flagSeries,
                        onSeries: seriesIds.singleValueChart.measurement,
                        showInLegend: false,
                        useHTML: true,
                        dataLabels: {
                            useHTML: true,
                        },
                        lineWidth: 1,
                        lineColor: '#005F87',
                        stackDistance: 10
                    }
                );
                ser.push(
                    {
                        id: seriesIds.singleValueChart.nominalValue + " " + input[i].variableI,
                        name: getLocalizedText('nominalValue', settings.locale) + " " + input[i].variableI, //  Highcharts.uiLocale[settings.locale].nominalValue,
                        animation: settings.animation,
                        data: input[i].nominalValue,
                        yAxis: 0,
                        color: chartSeriesColors.SingleValueChart.nominalValue,
                        step: 'left',
                        tooltip: {
                            valueDecimals: settings.decimalPlaces
                        },
                        marker: {
                            enabled: false
                        },
                        lineWidth: 1.2,
                        legendIndex: 22,
                        showInLegend: seriesVisibility(seriesIds.singleValueChart.nominalValue, input[i].nominalValue, settings.hiddenSeries, true).showInLegend,
                        visible: seriesVisibility(seriesIds.singleValueChart.nominalValue, input[i].nominalValue, settings.hiddenSeries, true).showInChart,
                        events: {
                            click: onSeriesClick
                        }
                    }
                );


                //==============================
                //  first, second and third choise (for future use)
                //==============================
                if (false) { 
                //ser.push(
                //    {
                //        id: seriesIds.singleValueChart.firstChoiceUpperLimit + " " + input[i].variableI,
                //        name: getLocalizedText('firstChoiceUpperLimit', settings.locale) + " " + input[i].variableI,
                //        animation: settings.animation,
                //        data: input[i].data.firstChoiceUpperLimit,
                //        yAxis: 0,
                //        color: chartSeriesColors.SingleValueChart.FirstChoice,
                //        step: 'left',
                //        tooltip: {
                //            valueDecimals: settings.decimalPlaces
                //        },
                //        marker: {
                //            enabled: false
                //        },
                //        lineWidth: 1.2,
                //        legendIndex: 9,
                //        showInLegend: seriesVisibility(seriesIds.singleValueChart.firstChoiceUpperLimit, input[i].firstChoiceUpperLimit, settings.hiddenSeries, false).showInLegend,
                //        visible: seriesVisibility(seriesIds.singleValueChart.firstChoiceUpperLimit, input[i].firstChoiceUpperLimit, settings.hiddenSeries, false).showInChart,
                //        events: {
                //            click: onSeriesClick
                //        }
                //    }
                //);
                //ser.push(
                //    {
                //        id: seriesIds.singleValueChart.firstChoiceLowerLimit + " " + input[i].variableI,
                //        linkedTo: seriesIds.singleValueChart.firstChoiceUpperLimit + " " + input[i].variableI,
                //        //name: getLocalizedText('firstChoiceLowerLimit', settings.locale),
                //        animation: settings.animation,
                //        data: input[i].firstChoiceLowerLimit,
                //        yAxis: 0,
                //        color: chartSeriesColors.SingleValueChart.FirstChoice,
                //        step: 'left',
                //        tooltip: {
                //            valueDecimals: settings.decimalPlaces
                //        },
                //        marker: {
                //            enabled: false
                //        },
                //        lineWidth: 1.2,
                //        legendIndex: 11,
                //        events: {
                //            click: onSeriesClick
                //        }
                //    }
                //);
                //ser.push(
                //    {
                //        id: seriesIds.singleValueChart.secondChoiceUpperLimit + " " + input[i].variableI,
                //        name: getLocalizedText('secondChoiceUpperLimit', settings.locale),
                //        animation: settings.animation,
                //        data: input[i].secondChoiceUpperLimit,
                //        yAxis: 0,
                //        color: chartSeriesColors.SingleValueChart.SecondChoice,
                //        step: 'left',
                //        tooltip: {
                //            valueDecimals: settings.decimalPlaces
                //        },
                //        marker: {
                //            enabled: false
                //        },
                //        lineWidth: 1.2,
                //        legendIndex: 12,
                //        showInLegend: seriesVisibility(seriesIds.singleValueChart.secondChoiceUpperLimit, input[i].secondChoiceUpperLimit, settings.hiddenSeries, false).showInLegend,
                //        visible: seriesVisibility(seriesIds.singleValueChart.secondChoiceUpperLimit, input[i].secondChoiceUpperLimit, settings.hiddenSeries, false).showInChart,
                //        events: {
                //            click: onSeriesClick
                //        }
                //    }
                //);
                //ser.push(
                //    {
                //        id: seriesIds.singleValueChart.secondChoiceLowerLimit + " " + input[i].variableI,
                //        linkedTo: seriesIds.singleValueChart.secondChoiceUpperLimit + " " + input[i].variableI,
                //        //name: getLocalizedText('secondChoiceLowerLimit', settings.locale), 
                //        animation: settings.animation,
                //        data: input[i].secondChoiceLowerLimit,
                //        yAxis: 0,
                //        color: chartSeriesColors.SingleValueChart.SecondChoice,
                //        step: 'left',
                //        tooltip: {
                //            valueDecimals: settings.decimalPlaces
                //        },
                //        marker: {
                //            enabled: false
                //        },
                //        lineWidth: 1.2,
                //        legendIndex: 13,
                //        events: {
                //            click: onSeriesClick
                //        }
                //    }
                //);
                //ser.push(
                //    {
                //        id: seriesIds.singleValueChart.thirdChoiceUpperLimit + " " + input[i].variableI,
                //        name: getLocalizedText('thirdChoiceUpperLimit', settings.locale),
                //        animation: settings.animation,
                //        data: input[i].thirdChoiceUpperLimit,
                //        yAxis: 0,
                //        color: chartSeriesColors.SingleValueChart.ThirdChoice,
                //        step: 'left',
                //        tooltip: {
                //            valueDecimals: settings.decimalPlaces
                //        },
                //        marker: {
                //            enabled: false
                //        },
                //        lineWidth: 1.2,
                //        legendIndex: 14,
                //        showInLegend: seriesVisibility(seriesIds.singleValueChart.thirdChoiceUpperLimit, input[i].thirdChoiceUpperLimit, settings.hiddenSeries, false).showInLegend,
                //        visible: seriesVisibility(seriesIds.singleValueChart.thirdChoiceUpperLimit, input[i].thirdChoiceUpperLimit, settings.hiddenSeries, false).showInChart,
                //        events: {
                //            click: onSeriesClick
                //        }
                //    }
                //);
                //ser.push(
                //    {
                //        id: seriesIds.singleValueChart.thirdChoiceLowerLimit + " " + input[i].variableI,
                //        linkedTo: seriesIds.singleValueChart.thirdChoiceUpperLimit + " " + input[i].variableI,
                //        //name: getLocalizedText('thirdChoiceLowerLimit', settings.locale), 
                //        animation: settings.animation,
                //        data: input[i].thirdChoiceLowerLimit,
                //        yAxis: 0,
                //        color: chartSeriesColors.SingleValueChart.ThirdChoice,
                //        step: 'left',
                //        tooltip: {
                //            valueDecimals: settings.decimalPlaces
                //        },
                //        marker: {
                //            enabled: false
                //        },
                //        lineWidth: 1.2,
                //        legendIndex: 15,
                //        events: {
                //            click: onSeriesClick
                //        }
                //    }
                //);

            }

                //==============================
                //  more unused series (for future use)
                //==============================
                if (false) {


                //    ser.push(
                //        {
                //            id: seriesIds.controlChart.centerOfTolerances + " " + input[i].variableI,
                //            name: getLocalizedText('centerOfTolerances', settings.locale) + " " + input[i].variableI,
                //            animation: settings.animation,
                //            data: input[i].centerOfTolerances,
                //            yAxis: 0,
                //            color: chartSeriesColors.SingleValueChart.CenterOfTolerances,
                //            step: 'left',
                //            tooltip: {
                //                valueDecimals: settings.decimalPlaces
                //            },
                //            marker: {
                //                enabled: false
                //            },
                //            lineWidth: 1.2,
                //            legendIndex: 16,
                //            showInLegend: seriesVisibility(seriesIds.singleValueChart.centerOfTolerances, input[i].centerOfTolerances, settings.hiddenSeries, false).showInLegend,
                //            visible: seriesVisibility(seriesIds.singleValueChart.centerOfTolerances, input[i].centerOfTolerances, settings.hiddenSeries, false).showInChart,
                //            events: {
                //                click: onSeriesClick
                //            }
                //        }
                //    );
                //    ser.push(
                //        {
                //            id: seriesIds.singleValueChart.averageOfAllValues + " " + input[i].variableI,
                //            name: getLocalizedText('averageOfAllValues', settings.locale) + " " + input[i].variableI,
                //            animation: settings.animation,
                //            data: input[i].averageOfAllValues,
                //            yAxis: 0,
                //            color: chartSeriesColors.SingleValueChart.averageOfAllValues,
                //            step: 'left',
                //            tooltip: {
                //                valueDecimals: settings.decimalPlaces
                //            },
                //            marker: {
                //                enabled: false
                //            },
                //            lineWidth: 1.2,
                //            legendIndex: 17,
                //            showInLegend: seriesVisibility(seriesIds.singleValueChart.averageOfAllValues, input[i].averageOfAllValues, settings.hiddenSeries, false).showInLegend,
                //            visible: seriesVisibility(seriesIds.singleValueChart.averageOfAllValues, input[i].averageOfAllValues, settings.hiddenSeries, false).showInChart,
                //            events: {
                //                click: onSeriesClick
                //            }
                //        }
                //    );
                //    ser.push(
                //        {
                //            id: seriesIds.singleValueChart.oneThirdOfUpperControlLimit + " " + input[i].variableI,
                //            name: getLocalizedText('oneThirdOfControlLimits', settings.locale) + " " + input[i].variableI,
                //            animation: settings.animation,
                //            data: input[i].oneThirdOfUpperControlLimit,
                //            yAxis: 0,
                //            color: chartSeriesColors.SingleValueChart.oneThirdOfControlLimits,
                //            step: 'left',
                //            tooltip: {
                //                valueDecimals: settings.decimalPlaces
                //            },
                //            marker: {
                //                enabled: false
                //            },
                //            lineWidth: 1.2,
                //            legendIndex: 18,
                //            showInLegend: seriesVisibility(seriesIds.singleValueChart.oneThirdOfUpperControlLimit, input[i].oneThirdOfUpperControlLimit, settings.hiddenSeries, false).showInLegend,
                //            visible: seriesVisibility(seriesIds.singleValueChart.oneThirdOfUpperControlLimit, input[i].oneThirdOfUpperControlLimit, settings.hiddenSeries, false).showInChart,
                //            events: {
                //                click: onSeriesClick
                //            }
                //        }
                //    );
                //    ser.push(
                //        {
                //            id: seriesIds.singleValueChart.twoThirdsOfUpperControlLimit + " " + input[i].variableI,
                //            name: getLocalizedText('twoThirdsOfControlLimits', settings.locale) + " " + input[i].variableI,
                //            animation: settings.animation,
                //            data: input[i].twoThirdsOfUpperControlLimit,
                //            yAxis: 0,
                //            color: chartSeriesColors.SingleValueChart.twoThirdsOfControlLimits,
                //            step: 'left',
                //            tooltip: {
                //                valueDecimals: settings.decimalPlaces
                //            },
                //            marker: {
                //                enabled: false
                //            },
                //            lineWidth: 1.2,
                //            legendIndex: 19,
                //            showInLegend: seriesVisibility(seriesIds.singleValueChart.twoThirdsOfUpperControlLimit, input[i].twoThirdsOfUpperControlLimit, settings.hiddenSeries, false).showInLegend,
                //            visible: seriesVisibility(seriesIds.singleValueChart.twoThirdsOfUpperControlLimit, input[i].twoThirdsOfUpperControlLimit, settings.hiddenSeries, false).showInChart,
                //            events: {
                //                click: onSeriesClick
                //            }
                //        }
                //    );
                //    ser.push(
                //        {
                //            id: seriesIds.singleValueChart.oneThirdOfLowerControlLimit + " " + input[i].variableI,
                //            linkedTo: seriesIds.singleValueChart.oneThirdOfUpperControlLimit + " " + input[i].variableI,
                //            //name: getLocalizedText('oneThirdOfLowerControlLimit', settings.locale), 
                //            animation: settings.animation,
                //            data: input[i].oneThirdOfLowerControlLimit,
                //            yAxis: 0,
                //            color: chartSeriesColors.SingleValueChart.oneThirdOfControlLimits,
                //            step: 'left',
                //            tooltip: {
                //                valueDecimals: settings.decimalPlaces
                //            },
                //            marker: {
                //                enabled: false
                //            },
                //            lineWidth: 1.2,
                //            legendIndex: 20,
                //            events: {
                //                click: onSeriesClick
                //            }
                //        }
                //    );
                //    ser.push(
                //        {
                //            id: seriesIds.singleValueChart.twoThirdsOfLowerControlLimit + " " + input[i].variableI,
                //            linkedTo: seriesIds.singleValueChart.twoThirdsOfUpperControlLimit + " " + input[i].variableI,
                //            //name: getLocalizedText('twoThirdsOfLowerControlLimit', settings.locale),
                //            animation: settings.animation,
                //            data: input[i].twoThirdsOfLowerControlLimit,
                //            yAxis: 0,
                //            color: chartSeriesColors.SingleValueChart.twoThirdsOfControlLimits,
                //            step: 'left',
                //            tooltip: {
                //                valueDecimals: settings.decimalPlaces
                //            },
                //            marker: {
                //                enabled: false
                //            },
                //            lineWidth: 1.2,
                //            legendIndex: 21,
                //            events: {
                //                click: onSeriesClick
                //            }
                //        }
                //    );
                }
            }
        }
        return ser;
    }

    /**
     * @description The series click callback event.
     * @memberof MultiVarSingleValueChart
     * @function onSeriesClick
     * @param {object} data - The object hold the information of clicked point.
     * @param {number} data.sequenceId - The corresponding sequence id of the clicked point.
     * @param {string} data.seriesName - The clicked series name.
     * @param {number} data.point - The clicked point value
     * @param {string} data.seriesId - The id of the clicked series.
     * @example
     * // When a callback is registered with options in single value chart options
     *  $('#svc').singleValueChart(
     *                  {
     *                      locale: "en",
     *                      data: {
     *                          specification: {
    *                                   controlChartType : 'xb'
    *                               },
     *                          measurements : [], // Array of objects in defined format
     *                          subgroups: [],  // Array of objects in defined format
     *                      },
     *                      decimalPlaces: 2,
     *                      hiddenSeries: ["lcl"],
     *                      onSeriesClick: function (data) {
     *                                          console.log(data);
     *                                          //The output will be like this
     *                                          // sequenceId: 484, seriesName: "measurements__1", point: 19.99, seriesId: "measurements__1"
     *                                          }
     *                   }
     *                );
     *

     */
    function onSeriesClick(e) {
        if (typeof settings.onSeriesClick === 'function') { // if callback is defined
            var sequenceId,
                point,
                seriesID,
                seriesName,
                output,
                referenceId;

            if (e && e.point && typeof e.point.x === 'number' && settings.data.measurements[e.point.x]) {
                sequenceId = settings.data.measurements[e.point.x].sequenceID;
                referenceId = settings.data.measurements[e.point.x].referenceID;

                if (e.point.series) {
                    seriesName = e.point.series.name;
                    point = e.point.y;
                    seriesID = e.point.series.options.id;
                }
            } else {
                if (e.point.series && e.point) {
                    seriesName = e.point.series.name;
                    point = e.point.y;
                    seriesID = e.point.series.options.id;
                }
            }
            output = { sequenceId: sequenceId, seriesName: seriesName, point: point, seriesId: seriesID, referenceId: referenceId };
            settings.onSeriesClick(output);
        }
    }

    /**
    * @description The legend click callback event.
    * @memberof SingleValueChart
    * @function onLegendClick
    * @param {Object} data - The object hold the information of clicked legend.
    * @param {string} data.seriesId - The series id of the clicked legend item.
    * @param {boolean} data.isVisible - Flag to represent the state of series that whether series was visible before click event or not.

    * @example
    * // When a callback is registered with options in single value chart options
    *  $('#svc').singleValueChart(
    *                  {
    *                      locale: "en",
    *                      data: {
    *                          specification: {
    *                                   controlChartType : 'xb'
    *                               },
    *                          measurements : [], // Array of objects in defined format
    *                          subgroups: [],  // Array of objects in defined format
    *                      },
    *                      decimalPlaces: 2,
    *                      hiddenSeries: ["lcl"],
    *                      onLegendClick : function (data) {
    *                                          console.log(data);
    *                                            //The output will be like this
    *                                            // seriesId: "lcl", isVisible: false
    *                                       }
    *                  }
    *              );
    *
    */
    function onLegendClick(e) {
        if (typeof settings.onLegendClick === 'function') {
            var seriesID,
                isVisible,
                output;
            seriesID = e.target.userOptions.id;
            isVisible = this.visible;
            output = { seriesId: seriesID, isVisible: isVisible };

            settings.onLegendClick(output);
        }
    }

    /**
    * @description The callback which is called when a chart is drawn. 
    * @memberof SingleValueChart
    * @callback onChartLoaded
    * @param {object} e - The event object with chart API object
    * @example
    * // When a callback is registered with options in single value chart options
    *  $('#svc').singleValueChart(
    *                  {
    *                      locale: "en",
    *                      data: {
    *                          specification: {
    *                                   controlChartType : 'xb'
    *                               },
    *                          measurements : [], // Array of objects in defined format
    *                          subgroups: [],  // Array of objects in defined format
    *                      },
    *                      decimalPlaces: 2,
    *                      onChartLoaded : function (data) {
    *                                          console.log(data);
    *                                       }
    *                  }
    *              );
    */
    function onChartLoaded(e) {
        if (typeof settings.onChartLoaded === 'function') { // if callback is defined
            settings.onChartLoaded(e);
        }
    }


    function getCustomxAxisLabel(customInfo) {
        if (customInfo && customInfo.filter) {
            var xAxisObj = customInfo.filter(function (k) { return k.xAxisLabel === true; });
            if (xAxisObj && xAxisObj.length > 0 && xAxisObj[0].value)
                return xAxisObj[0].value;
        }
        return '';
    }

    function getCustomTooltip(customInfo) {
        var customTooltip = [];

        if (customInfo && customInfo.filter) {
            var xAxisObj = customInfo.filter(function (k) { return k.showInTooltip === true; });
            if (xAxisObj) {
                xAxisObj.forEach(function (o) {
                    //customTooltip.push(o.label + ' : ' + o.value);
                    customTooltip.push(tooltipRow.format(o.label, o.value, 'customInfo'));
                });
            }
        }
        return customTooltip.join('');
    }

    function displayTooltip(/*tooltip*/) {
        //multiVarSingleValueChart
        var msg = [];

        msg.push(tooltipTableStart);

        msg.push(tooltipRow.format('<b>' + this.series.name + ' </b>', '<b> &nbsp;' + roundFloat(this.y, settings.decimalPlaces) + '</b>'));

        if (this.series.options.showCustomInfo) {
            msg.push(tooltipHr);
            msg.push(getCustomTooltip(findCustomInfo(this.series.options.id, this.point.index)));
        }
        if ((this.key.toString().startsWith('violation') || this.key.toString().startsWith('outlier') || this.key.toString().startsWith('eliminated')) && this.point.violations) {
            msg.push(tooltipHr);
            this.point.violations.forEach(function (v) { msg.push(tooltipRow.format(v, "")); });
        } else if (this.series.name === seriesIds.singleValueChart.flagSeries && this.point) {
            msg = [];
            msg.push(getCustomRemarks(settings, this.point.x));
        }

        if (this.point.text) // for flags type series, only it should be shown
            return "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:400'>" + this.point.text + "</span></div>";

        msg.push(tooltipTableEnd);

        var html = '<div style="padding:8px 16px"><span style="font-size:9pt;font-weight:400">' + msg.join('') + '</span></div>';
        //return tooltip.defaultFormatter.call(this, tooltip);
        return html;
    }
 
    function setDefaults(_options) {
        var opt = undefined;
        if (_options && typeof _options === 'object') {
            //_options.siemensTooltip = $.extend({}, siemensTooltip);
            //_options.siemensTooltip.formatter = displayTooltip;
            opt = $.extend({}, _options);

            opt.chartType = chartTypes.svc; // cannot be changed.
            opt.siemensTooltip = $.extend({}, siemensTooltip);
            opt.siemensTooltip.formatter = displayTooltip;
            if (!opt.locale || !Highcharts.uiLocale[opt.locale]) {
                console.error('Locale ' + opt.locale + ' not found, default English locales will be used');
                opt.locale = 'en';
            }
            if (typeof opt.yAxisUnit !== 'object') opt.yAxisUnit = getDefaultyAxisUnitObj();
            else opt.yAxisUnit = $.extend(getDefaultyAxisUnitObj(), opt.yAxisUnit);
            if (_options.splitTooltip == false/* && _options.siemensTooltip*/) {
                opt.siemensTooltip.split = true; // will combined the tooltip in highstock
            }

            if (_options.fontSize && _options.fontSize.labels) {
                axiesTitleStyle.fontSize = _options.fontSize.labels;
            }
            if (_options.fontSize && _options.fontSize.title) {
                titleStyle.fontSize = _options.fontSize.title;
            }

            if (_options.titleSettings) {
                _options.titleSettings.titleStyle = $.extend({}, titleStyle);
                _options.titleSettings.titleStyle.transform = 'translate(' + _options.titleSettings.x + 'px, ' + _options.titleSettings.y + 'px) rotate(' + _options.titleSettings.rotate + 'deg)'
            }

            if (!opt.series && typeof opt.series !== 'object') opt.series = getDefaultSeriesObj();
            else opt.series = $.extend(getDefaultSeriesObj(), opt.series);

        }
        return opt;
    }


    function isSubequalMeasure(charttype) {
        var charttypes = ['x_ms', 'x_mr', 'mx_ms', 'mx_mr'];
        if (charttypes.indexOf(charttype) > -1)
            return true;
        return false;
    }

    function prepareYAxis() {
        var unit = settings.yAxisUnit && isNullOrUndefined(settings.yAxisUnit.text) ? '' : settings.yAxisUnit.text;
        var yAxis =
            [
                {
                    // tickInterval: 0.008,                    
                    lineColor: siemensColors.PL_BLACK_22,
                    lineWidth: 2,
                    min: getYaxisMinScaling(),
                    max: getYaxisMaxScaling(),
                    opposite: false,
                    labels: {
                        formatter: function () {
                            return roundFloat(this.value, settings.decimalPlaces);  // parseFloat(this.value).toFixed(settings.decimalPlaces)
                        },
                        style: axiesStyle
                    },
                    title: {
                        text: settings.yAxisTitle || getLocalizedText('SVC_yAxisTitle', settings.locale), // Highcharts.uiLocale[settings.locale].SVC_yAxisTitle,
                        style: axiesTitleStyle
                    }
                },
                {// to show units of chart
                    opposite: false,
                    title: {
                        reserveSpace: false,
                        text: unit,
                        align: 'high',
                        rotation: 0,
                        x: settings.yAxisUnit.x,
                        y: settings.yAxisUnit.y,
                        style: {
                            fontSize: settings.yAxisUnit.fontSize,
                            color: siemensColors.PLBlack4
                        }
                    }
                }

            ];

        return yAxis;
    }

    function prepareChartOptions(input) {
        var options =
        {
            chart: {
                type: 'line',
                animation: settings.animation,
                height: settings.height,
                width: settings.width,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                },
                events: {
                    load: onChartLoaded
                }
            },
            credits: {
                enabled: false
            },
            plotOptions: {
                series: {
                    animation: settings.animation,
                    states: {
                        inactive: {
                            opacity: 0.8
                        }
                    },
                    marker: {
                        states: {
                            hover: {
                                enabled: false
                            }
                        }
                    },
                    events: {
                        legendItemClick: onLegendClick
                    }
                }
                //,
                //line: {
                //    events: {
                //        legendItemClick: onLegendClick
                //    }
                //}
            },
            exporting: {
                enabled: false
            },
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            navigator: {
                baseSeries: 4,
                enabled: true, // to show or hide zoom level on x-axis
                xAxis: {
                    labels: {
                        formatter: function () {
                            return this.value;
                        }
                        // enabled: true
                    }
                },
                series: {
                    lineColor: chartSeriesColors.SingleValueChart.Measurement
                }
            },
            rangeSelector: {
                enabled: false
                // selected: 5  // date, month year etc
            },
            title: {
                text: settings.chartTitle,
                align: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.align)) ? 'center' : settings.titleSettings.align,
                style: (isNullOrUndefined(settings.titleSettings) || isNullOrUndefined(settings.titleSettings.titleStyle)) ? titleStyle : settings.titleSettings.titleStyle,
            },
            tooltip: settings.siemensTooltip,
            xAxis: {
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                gridLineColor: siemensColors.PL_BLACK_22,
                // categories: input.sortNumbers,
                title: {
                    text: settings.xAxisTitle || getLocalizedText('SVC_xAxisTitle', settings.locale), // Highcharts.uiLocale[settings.locale].SVC_xAxisTitle,
                    style: axiesTitleStyle
                },
                labels: {
                    rotation: -90, // settings.xAxisLabelsRotation,
                    formatter: function () {
                        return input[0].sortNumbers[this.value];
                    },
                    style: axiesStyle
                },
                min: settings.navigator.start,
                max: settings.navigator.end,

                range: settings.navigator.selected // select last 25 samples
            },
            yAxis: prepareYAxis(),
            series: prepareSeries(input)
        };

        validateLegendPositioning(options.legend);

        return options;
    }

    function prepareColoredZone(varNo) {
        var zone = [];
        var violationZones = [];
        var interval;

        if (settings.data && settings.data.variablesResponse && settings.data.variablesResponse[varNo] && settings.data.variablesResponse[varNo].measurements) {
            var variable = settings.data.variablesResponse[varNo];
            var violatedSubgroupsNumber = [];

            $.each(variable.measurements, function (i, p) {
                var // subgroupSize = settings.data.specifications.subgroupSize > 0 ? settings.data.specifications.subgroupSize : 1,
                    subgrpNum = isSubequalMeasure(variable.specifications.controlChartType) ? i : p.subgroupNumber, //  parseInt(i / subgroupSize),
                    statuses = variable.subgroups && variable.subgroups[subgrpNum] && variable.subgroups[subgrpNum].statuses ? getVoilations(variable.subgroups[subgrpNum].statuses) : [];
                if (statuses.indexOf("involvedbyother") > -1) {
                    violatedSubgroupsNumber.push(roundFloat(p.sequenceID));
                }

            });

        }
        interval = getSequence(violatedSubgroupsNumber)
        violationZones = getColoredSeriesZone(interval, siemensColors.violationZone);
        violationZones.forEach(function (violationZone) {
            zone.push(violationZone);

        });
        return zone;


    }

    function getDefaultyAxisUnitObj() {
        var obj = {
            text: '',
            x: 60,
            y: undefined,
            fontSize: undefined
        };
        return obj;
    }

    function getMeasurmentStatuses(measurement, status) {
        var statuses = [];
        if (measurement && measurement.statuses && measurement.statuses.filter) {
            statuses = measurement.statuses.filter(function (s) {
                if (s.category && s.category.toLowerCase() === status) {
                    return true;
                }
            });
        }

        return statuses;
    }

    function getCustomRemarks(settings, index) {
        var customTooltip = [];
        var customInfo = undefined;
        if (settings.data && settings.data.measurements && settings.data.measurements[index]) {
            customInfo = settings.data.measurements[index].customInfo;
        }

        if (customInfo) {
            var xAxisObj = customInfo.filter(function (k) { return k.isRemark === true; });
            if (xAxisObj) {
                xAxisObj.forEach(function (o) {
                    customTooltip.push(o.value);
                });
            }
        }

        return getLocalizedText('annotation', settings.locale) + ' : ' + customTooltip.join(', ');
    }

    function getYaxisMinScaling() {
        if (!isNullOrUndefined(settings.yAxisScale)) {
            if (!isNullOrUndefined(settings.yAxisScale.min))
                return settings.yAxisScale.min;
            if (!isNullOrUndefined(settings.yAxisScale.minScaling))
                return getYaxisMinValue(settings.yAxisScale.minScaling)

        }
        return null;
    }
    function getYaxisMaxScaling() {
        if (!isNullOrUndefined(settings.yAxisScale)) {
            if (!isNullOrUndefined(settings.yAxisScale.max))
                return settings.yAxisScale.max;
            if (!isNullOrUndefined(settings.yAxisScale.maxScaling))
                return getYaxisMaxValue(settings.yAxisScale.maxScaling)

        }
        return undefined;
    }
    function getYaxisMinValue(scalingType) {
        var minValue = null;
        switch (scalingType) {
            case scaling.controlLimit:
                var lcl = settings.inputs.lcl.reduce(function (a, b) {
                    return Math.min(a, b);
                });
                var ucl = settings.inputs.ucl.reduce(function (a, b) {
                    return Math.min(a, b);
                });
                minValue = Math.min(lcl, ucl);
                break;
            case scaling.toleranceLimit:
                var ltl = settings.inputs.ltl.reduce(function (a, b) {
                    return Math.min(a, b);
                });
                var utl = settings.inputs.utl.reduce(function (a, b) {
                    return Math.min(a, b);
                });
                minValue = Math.min(lcl, ucl);
                break;

        }
        return minValue;
    }
    function getYaxisMaxValue(scalingType) {
        var maxValue = null;
        switch (scalingType) {
            case scaling.controlLimit:
                var lcl = settings.inputs.lcl.reduce(function (a, b) {
                    return Math.max(a, b);
                });
                var ucl = settings.inputs.ucl.reduce(function (a, b) {
                    return Math.max(a, b);
                });
                maxValue = Math.max(lcl, ucl);
                break;
            case scaling.toleranceLimit:
                var ltl = settings.inputs.ltl.reduce(function (a, b) {
                    return Math.max(a, b);
                });
                var utl = settings.inputs.utl.reduce(function (a, b) {
                    return Math.max(a, b);
                });
                maxValue = Math.max(ltl, utl);
                break;

        }
        return maxValue;
    }

    function getDefaultSeriesObj() {
        var obj =
        {
            tooltip: []
        };

        return obj;
    }

    function addAdditionalLines(input, subgroup, hasSubgroupAdditionalLines) {

        if (hasSubgroupAdditionalLines) {
            if (subgroup && subgroup.additionalLines) {
                input.firstChoiceUpperLimit.push(roundFloat(subgroup.additionalLines.firstChoiceUpperLimit, null, true));
                input.firstChoiceLowerLimit.push(roundFloat(subgroup.additionalLines.firstChoiceLowerLimit, null, true));
                input.secondChoiceUpperLimit.push(roundFloat(subgroup.additionalLines.secondChoiceUpperLimit, null, true));
                input.secondChoiceLowerLimit.push(roundFloat(subgroup.additionalLines.secondChoiceLowerLimit, null, true));
                input.thirdChoiceUpperLimit.push(roundFloat(subgroup.additionalLines.thirdChoiceUpperLimit, null, true));
                input.thirdChoiceLowerLimit.push(roundFloat(subgroup.additionalLines.thirdChoiceLowerLimit, null, true));
                input.centerOfTolerances.push(roundFloat(subgroup.additionalLines.centerOfTolerances, null, true));
                input.oneThirdOfUpperControlLimit.push(roundFloat(subgroup.additionalLines.oneThirdOfUpperControlLimit, null, true));
                input.twoThirdsOfUpperControlLimit.push(roundFloat(subgroup.additionalLines.twoThirdsOfUpperControlLimit, null, true));
                input.oneThirdOfLowerControlLimit.push(roundFloat(subgroup.additionalLines.oneThirdOfLowerControlLimit, null, true));
                input.twoThirdsOfLowerControlLimit.push(roundFloat(subgroup.additionalLines.twoThirdsOfLowerControlLimit, null, true));

                if (input.data.additionalLines)// is not provided by subgroup
                    input.averageOfAllValues.push(roundFloat(input.data.additionalLines.averageOfAllValues, null, true));
                else
                    input.averageOfAllValues.push(null);
            }
            else {
                input.firstChoiceUpperLimit.push(null);
                input.firstChoiceLowerLimit.push(null);
                input.secondChoiceUpperLimit.push(null);
                input.secondChoiceLowerLimit.push(null);
                input.thirdChoiceUpperLimit.push(null);
                input.thirdChoiceLowerLimit.push(null);

                input.centerOfTolerances.push(null);
                input.averageOfAllValues.push(null);
                input.oneThirdOfUpperControlLimit.push(null);
                input.twoThirdsOfUpperControlLimit.push(null);
                input.oneThirdOfLowerControlLimit.push(null);
                input.twoThirdsOfLowerControlLimit.push(null);
            }
        } else if (input.data.additionalLines) {
            input.firstChoiceUpperLimit.push(roundFloat(input.data.additionalLines.firstChoiceUpperLimit, null, true));
            input.firstChoiceLowerLimit.push(roundFloat(input.data.additionalLines.firstChoiceLowerLimit, null, true));
            input.secondChoiceUpperLimit.push(roundFloat(input.data.additionalLines.secondChoiceUpperLimit, null, true));
            input.secondChoiceLowerLimit.push(roundFloat(input.data.additionalLines.secondChoiceLowerLimit, null, true));
            input.thirdChoiceUpperLimit.push(roundFloat(input.data.additionalLines.thirdChoiceUpperLimit, null, true));
            input.thirdChoiceLowerLimit.push(roundFloat(input.data.additionalLines.thirdChoiceLowerLimit, null, true));

            input.centerOfTolerances.push(roundFloat(input.data.additionalLines.centerOfTolerances, null, true));
            input.averageOfAllValues.push(roundFloat(input.data.additionalLines.averageOfAllValues, null, true));
            input.oneThirdOfUpperControlLimit.push(roundFloat(input.data.additionalLines.oneThirdOfUpperControlLimit, null, true));
            input.twoThirdsOfUpperControlLimit.push(roundFloat(input.data.additionalLines.twoThirdsOfUpperControlLimit, null, true));
            input.oneThirdOfLowerControlLimit.push(roundFloat(input.data.additionalLines.oneThirdOfLowerControlLimit, null, true));
            input.twoThirdsOfLowerControlLimit.push(roundFloat(input.data.additionalLines.twoThirdsOfLowerControlLimit, null, true));
        }
    }

    function findCustomInfo(variableId, measurementId) {
        var myCustomInfo;
        var myTool;
        if (variableId && measurementId >= 0 && settings.data && settings.data.variablesResponse && settings.data.variablesResponse.length > 0) {
            myTool = findActualIdFromMultiVar(variableId, settings.data.variablesResponse);
            myCustomInfo = myTool.measurements[measurementId].customInfo;
        }
        return myCustomInfo;
    }
    function findActualIdFromMultiVar(variableId, variableArray) {
        var myId;
        var idLength = 0;
        for (var j = 0; j < variableArray.length; j++) {
            myId = variableArray[j].variableId;
            idLength = myId.length;
            if (myId == variableId.substr(variableId.length - idLength)) {
                return variableArray[j];
            }

        }
    }
};
/**
 * @description Provide methods for drawing Multi Attributive Defect Pareto chart .
 *
 * @namespace MultiAttDefectPareto
 * */
/**
 * @description Draw a Multi Attributive  chart using the provided data from the options parameter
 * @memberof MultiAttDefectPareto
 * @function multiAttDefectPareto
 * @requires highstock.js
 * @requires jQuery.js
 * @param {MultiAttDefectPareto.options} options - The chart options parameter
 * @example
 *  $('#mAttdefPar').multiAttDefectPareto({
            locale: locale,
            data: data, // JSON Array with calculation response
            chartTitle: 'MultiAttDefectPareto',          
            decimalPlaces: 4,
            height: 600,           
             xAxisTitle: undefined,
             yAxisTitle: undefined
        });
*/
/**
 * @description the legend position settings
 * @typedef {object} legendSettings
 * @memberof MultiAttDefectPareto
 * @property {boolean} [legendSettings.enabled = true] - show or hide the legend.
 * @property {string} [legendSettings.align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [legendSettings.verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [legendSettings.layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [legendSettings.x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [legendSettings.Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */
/**
 * @memberof MultiAttDefectPareto
 * @typedef {object} options
 * @property {MultiAttDefectPareto.data} data  Array of attributives Pareto and if Visual -- the sum of all defects, for the Visual Characteristic    {@link pieChart.data}
 * @property {string} [options.locale=en]  User locale
 * @property {MultiAttDefectPareto.data} options.data - JSON Array with Attributives , specifications
 * @property {MultiAttDefectPareto.legendSettings} legendSettings - Settings for position, layout and visibility of the chart legend
 * @property {string} options.chartTitle The chart's main title.
 * @property {number} [options.decimalPlaces = 4] - The number of digits to appear after the decimal point.
 * @property {number} [options.height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number} [options.width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string} [options.xAxisTitle=null] - The x-Axis label displayed underneath the x-axis.
 * @property {boolean}  [animation=false] -Enable or disable the initial animation
 * @property {MultiAttDefectPareto.fontSize} [fontSize]-Option to set Fontsize of label & title.
*/
/**
 * @typedef {object} fontSize
 * @memberof MultiAttDefectPareto
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */
$.fn.multiAttDefectPareto = function (options) {
    setDefaults();
    if (options && typeof options === 'object') {
        var settings = $.extend({
            locale: 'en',
            data: {},
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            chartTitle: undefined,
            xAxisTitle: getLocalizedText('Defect_Pareto_xAxis', options.locale),
            yAxisTitle: getLocalizedText('Defect_Pareto_yAxis', options.locale),
            legendSettings: {
                enabled: true,
                align: 'right',
                verticalAlign: 'top',
                layout: 'vertical',
                x: 0,
                y: 100
            },
            animation:false
        }, options);
    } else if (!options)
        throw 'Not valid options or parameter to the "chart" widget passed';
    var $this = this,
        _args = arguments,
        chartOption = createChartOptions(settings),
        chart = {
            init: function () {
                var containerId = $this.attr('id');
                //var _chart =
                Highcharts.chart(containerId, chartOption);
            },

            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof MultiAttDefectPareto
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered.
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#multiAttDefectPareto').multiAttDefectPareto('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64',
            *                                           width : 1800,
            *                                           height: 600,
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        };
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function createChartOptions(settings) {
        var input = {
            categories: [],
            defectsCount: []
        },
            siemens_tooltip = $.extend({}, siemensTooltip);

        prepareData(input);
        var optionValues = {
            chart: {
                height: settings.height,
                width: settings.width,
                animation: settings.animation,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                },
                type: 'column'
            },
            title: {
                text: options.chartTitle || getLocalizedText('Pareto_chart_title', options.locale),
                align: 'center',
                style: titleStyle
            },
            credits: {
                enabled: false
            },
            tooltip: siemens_tooltip, // siemensTooltip,
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            yAxis: {
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                labels: {
                    enabled: true,
                    style: axiesStyle,
                    formatter: function () {
                        return this.value + ' %'
                    }
                },
                title: {
                    text: settings.yAxisTitle, //  options.yAxisTitle || getLocalizedText('Defect_Pareto_yAxis', options.locale), 
                    style: axiesTitleStyle
                },
            },
            xAxis: {
                title: {
                    text: settings.xAxisTitle, //  options.xAxisTitle || getLocalizedText('Defect_Pareto_xAxis', options.locale), 
                    style: axiesTitleStyle
                },
                gridLineColor: siemensColors.PL_BLACK_22,
                categories: input.categories,
                labels: {
                    style: axiesStyle
                }
            },
            plotOptions: {
                column: {
                    stacking: 'normal',
                    animation: settings.animation,
                    dataLabels: {
                        enabled: false
                    }
                },
                series: {
                    animation: settings.animation
                }
            },
            series: prepareSeries(input)
            
        };
        validateLegendPositioning(optionValues.legend);

        return optionValues;
    }
    function prepareData(input) {       
            $.each(settings.data.attributivesPareto, function (index, defect) {
                input.categories.push(roundFloat(defect.attributiveID));
                input.defectsCount.push(roundFloat(defect.percentage));
            });        
       
    }
    function prepareSeries(input) {      
            var ser = [
                {
                    name: 'Defects',
                    data: input.defectsCount,
                    color: chartSeriesColors.Pareto.DefectsCount,
                    animation: settings.animation,
                    states: {
                        hover: {
                            color: chartSeriesColors.Pareto.DefectsCounthover
                        }
                    }
                }
            ];       
        return ser;
    }
   
    function displayTooltip(/*tooltip*/) {
        if (settings.data) {          
                var msg = "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:600'>" + this.series.name + ": </span><span style='font-size:9pt;font-weight:400'>" + roundFloat(this.y, settings.decimalPlaces) + "</span></div>";
                var key = this.x;
                var defectObj = settings.data.attributivesPareto.filter(function (obj) {
                    return obj.attributiveID === key;
                });
                if (defectObj[0]) {
                    var defectIDcaption = "<span style='font-size: 9pt; font-weight: 600'>" + getLocalizedText('Defect_Pareto_ID_Tooltip_Caption', settings.locale) + ":</span><span style='font-size: 9pt; font-weight: 400'> " + roundFloat(defectObj[0].attributiveID) + "</span><br>";
                    var defectCountCaption = "<span style='font-size: 9pt; font-weight: 600'>" + getLocalizedText('Defect_Pareto_count_Tooltip_Caption', settings.locale) + ":</span><span style='font-size: 9pt; font-weight: 400'> " + roundFloat(defectObj[0].numberOfDefects) + "</span><br>";
                    var defectPercentCaption = "<span style='font-size: 9pt; font-weight: 600'>" + getLocalizedText('Defect_Pareto_percent_Tooltip_Caption', settings.locale) + ":</span><span style='font-size: 9pt; font-weight: 400'> " + roundFloat(defectObj[0].percentage, settings.decimalPlaces) + " %</span><br>";
                    msg = "<div style='padding:8px 16px'>" + defectIDcaption + defectCountCaption + defectPercentCaption  + "</div>";
                }       
        }
        return msg;
    }
    function setDefaults() {
        if (typeof options === 'object') {
            siemensTooltip.formatter = displayTooltip;
            if (!options.locale || !Highcharts.uiLocale[options.locale]) {
                console.error('Locale ' + options.locale + ' not found, default English locales will be used');
                options.locale = 'en';
            }

            if (options.fontSize && options.fontSize.labels) {
                axiesTitleStyle.fontSize = options.fontSize.labels;
            }
            if (options.fontSize && options.fontSize.title) {
                titleStyle.fontSize = options.fontSize.title;
            }
        }
    }
};
/**
 * @description Provide methods for drawing Multi Stacked Viual Defect Pareto chart .
 *
 * @namespace MultiStackedVisDefectPareto
 * */
/**
 * @description Draw a Multi Stacked Viual chart using the provided data from the options parameter
 * @memberof MultiStackedVisDefectPareto
 * @function multiStackedVisDefectPareto
 * @requires highstock.js
 * @requires jQuery.js
 * @param {MultiStackedVisDefectPareto.options} options - The chart options parameter
 * @example
 *  $('#mstVisdefPar').multiStackedVisDefectPareto({
            locale: locale,
            data: data, // JSON Array with defects
            chartTitle: 'MultiStackedVisDefectPareto',          
            decimalPlaces: 4,
            height: 600,           
             xAxisTitle: undefined,
             yAxisTitle: undefined
        });
*/
/**
 * @description the legend position settings
 * @typedef {object} legendSettings
 * @memberof MultiStackedVisDefectPareto
 * @property {boolean} [legendSettings.enabled = true] - show or hide the legend.
 * @property {string} [legendSettings.align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [legendSettings.verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [legendSettings.layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [legendSettings.x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [legendSettings.Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */
/**
 * @memberof MultiStackedVisDefectPareto
 * @typedef {object} options
 * @property {MultiStackedVisDefectPareto.data} data  Array of all single defects, for every Visual Characteristic and all other info   {@link pieChart.data}
 * @property {string} [options.locale=en]  User locale
 * @property {MultiStackedVisDefectPareto.data} options.data - JSON Array with Visual Defects , specifications
 * @property {MultiStackedVisDefectPareto.legendSettings} legendSettings - Settings for position, layout and visibility of the chart legend
 * @property {string} options.chartTitle The chart's main title.
 * @property {number} [options.decimalPlaces = 4] - The number of digits to appear after the decimal point.
 * @property {number} [options.height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number} [options.width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string} [options.xAxisTitle=null] - The x-Axis label displayed underneath the x-axis.
 * @property {boolean}  [animation=false] -Enable or disable the initial animation
 * @property {MultiStackedVisDefectPareto.fontSize} [fontSize]-Option to set Fontsize of label & title.
*/
/**
 * @typedef {object} fontSize
 * @memberof MultiStackedVisDefectPareto
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */
$.fn.multiStackedVisDefectPareto = function (options) {
    setDefaults();
    if (options && typeof options === 'object') {
        var settings = $.extend({
            locale: 'en',
            data: {},
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            chartTitle: undefined,
            xAxisTitle: getLocalizedText('Defect_Pareto_xAxis', options.locale),
            yAxisTitle: getLocalizedText('Defect_Pareto_yAxis', options.locale),
            legendSettings: {
                enabled: true,
                align: 'right',
                verticalAlign: 'top',
                layout: 'vertical',
                x: 0,
                y: 100
            },
            animation: false
        }, options);
    } else if (!options)
        throw 'Not valid options or parameter to the "chart" widget passed';
    var $this = this,
        _args = arguments,
        chartOption = createChartOptions(settings),
        chart = {
            init: function () {
                var containerId = $this.attr('id');
                //var _chart =
                Highcharts.chart(containerId, chartOption);
            },

            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof MultiStackedVisDefectPareto
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered.
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#multiStackedVisDefectPareto').multiStackedVisDefectPareto('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64',
            *                                           width : 1800,
            *                                           height: 600,
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        };
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function createChartOptions(settings) {
        var input = {
            categories: [],
            defectsCount: []
        },
            siemens_tooltip = $.extend({}, siemensTooltip);

        prepareData(input);
        var optionValues = {
            chart: {
                height: settings.height,
                width: settings.width,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                },
                type: 'column'
            },
            animation: settings.animation,
            title: {
                text: options.chartTitle || getLocalizedText('Pareto_chart_title', options.locale),
                align: 'center',
                style: titleStyle
            },
            credits: {
                enabled: false
            },
            tooltip: siemens_tooltip, // siemensTooltip,
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            yAxis: {
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                labels: {
                    enabled: true,
                    style: axiesStyle,
                    formatter: function () {
                        return this.value + ' %'
                    }
                },
                title: {
                    text: settings.yAxisTitle, //  options.yAxisTitle || getLocalizedText('Defect_Pareto_yAxis', options.locale), 
                    style: axiesTitleStyle
                },
            },
            xAxis: {
                title: {
                    text: settings.xAxisTitle, //  options.xAxisTitle || getLocalizedText('Defect_Pareto_xAxis', options.locale), 
                    style: axiesTitleStyle
                },
                gridLineColor: siemensColors.PL_BLACK_22,
                categories: input.categories,
                labels: {
                    style: axiesStyle
                }
            },
            plotOptions: {
                column: {
                    stacking: 'normal',
                    dataLabels: {
                        enabled: false
                    },
                    animation: settings.animation
                }
            },
            series: prepareSeries(input)

        };
        validateLegendPositioning(optionValues.legend);

        return optionValues;
    }
    function prepareData(input) {

        var defectIDList = [];
        $.each(settings.data.attributivesPareto, function (index, visual) {

            $.each(settings.data.defectsPareto, function (index, defect) {
                if (visual.attributiveID === defect.visualID) {
                    defectIDList.push(roundFloat(defect.defectID));
                }
                             
            });
            if (defectIDList.length > input.categories.length) {
                input.categories = defectIDList;  
            }
            defectIDList = [];
            
        });
    }
    function prepareSeries(input) {

        var colorData = [];
        var chartdata = {
            name: [],
            animation: settings.animation,
            data: [],          
            color: []
        };
       
        var ser = [];
        
        // colorData = Object.values(chartSeriesColors.MultiVisDefect);
        colorData = $.map(chartSeriesColors.MultiVisDefect, function (value, key) { return value; }) || [];
        //For each Characteristic
        $.each(settings.data.attributivesPareto, function (attIndex, visual) {
           
            if (colorData.length >= attIndex) {
                chartdata.color = colorData[attIndex];
            } else {
                chartdata = {
                    name: [],
                    animation: settings.animation,
                    data: []
                };
            }
            chartdata.name = visual.attributiveID;        
            
            //Get each defect
            $.each(settings.data.defectsPareto, function (visIndex, defect) {

                if (visual.attributiveID === defect.visualID) {
                 
                    if (input.categories[chartdata.data.length] === defect.defectID) {
                        chartdata.data.push(roundFloat(defect.percentage));
                    }
                    else {     
                        var currIndex = chartdata.data.length;
                        while (input.categories[currIndex] !== defect.defectID) {
                            chartdata.data.push(0);
                            currIndex = currIndex + 1;
                        }                        
                        chartdata.data.push(roundFloat(defect.percentage));
                    }

                }
            });
           
            ser.push(chartdata);
            chartdata = {
                name: [],
                animation: settings.animation,
                data: [],
                color: []
            };
        });
        //To be compliant with the UX specification
        ser.reverse();
        return ser;
    }
   

    function displayTooltip(/*tooltip*/) {
        if (settings.data) {
            
                var msg = "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:600'>" + this.series.name + ": </span><span style='font-size:9pt;font-weight:400'>" + roundFloat(this.y, settings.decimalPlaces) + "</span></div>";
                var defectkey = this.x;
                var visuelkey = this.series.name;
                var defectObj = settings.data.defectsPareto.filter(function (obj) {
                    return obj.defectID === defectkey && obj.visualID === visuelkey;
                });
                if (defectObj[0]) {
                    var defectIDcaption = "<span style='font-size: 9pt; font-weight: 600'>" + getLocalizedText('Defect_Pareto_ID_Tooltip_Caption', settings.locale) + ":</span><span style='font-size: 9pt; font-weight: 400'> " + roundFloat(defectObj[0].visualID) + "</span><br>";
                    var defectCountCaption = "<span style='font-size: 9pt; font-weight: 600'>" + getLocalizedText('Defect_Pareto_count_Tooltip_Caption', settings.locale) + ":</span><span style='font-size: 9pt; font-weight: 400'> " + roundFloat(defectObj[0].numberOfDefects) + "</span><br>";
                    var defectPercentCaption = "<span style='font-size: 9pt; font-weight: 600'>" + getLocalizedText('Defect_Pareto_percent_Tooltip_Caption', settings.locale) + ":</span><span style='font-size: 9pt; font-weight: 400'> " + roundFloat(defectObj[0].percentage, settings.decimalPlaces) + " %</span><br>";
                    var defectclass = "<span style='font-size: 9pt; font-weight: 600'>" + getLocalizedText('Defect_Pareto_class_Tooltip_Caption', settings.locale) + ":</span><span style='font-size: 9pt; font-weight: 400'> " + defectObj[0].classification + "</span><br>";
                    msg = "<div style='padding:8px 16px'>" + defectIDcaption + defectCountCaption + defectPercentCaption + defectclass + "</div>";
                }
            
        }
        return msg;
    }
    function setDefaults() {
        if (typeof options === 'object') {
            siemensTooltip.formatter = displayTooltip;
            if (!options.locale || !Highcharts.uiLocale[options.locale]) {
                console.error('Locale ' + options.locale + ' not found, default English locales will be used');
                options.locale = 'en';
            }

            if (options.fontSize && options.fontSize.labels) {
                axiesTitleStyle.fontSize = options.fontSize.labels;
            }
            if (options.fontSize && options.fontSize.title) {
                titleStyle.fontSize = options.fontSize.title;
            }
        }
    }
};
/**
 * @description Provide methods for drawing a bar chart .
 *
 * @namespace BarChart
 * */

/**
 * @description A new bar chart can be drawn using this method.
 * @memberof BarChart
 * @function BarChart
 * @requires highstock.js
 * @requires jQuery.js
 * @param {BarChart.options} options - The chart options parameter
 * @example
 * $('#BarChart').BarChart({
 *              data: {},
                decimalPlaces: 4,
                height: undefined,
                width: undefined,
                chartTitle: 'Bar',
                xAxisTitle: 'xAxis',
                yAxisTitle: 'yAxis',
                seriesName: 'series',
                seriesColor: '#50BED7',
                seriesHoverColor: '#0F789B',
                categoryTooltipCaption: 'category',
                countTooltipCaption: 'count',
                percentageTooltipCaption: 'percentage',
                classificationTooltipCaption:'classification'
 *          });
 */

/**
 * @memberof BarChart
 * @typedef {object} options
 * @property {BarChart.data} data  Array of defect in a defined format {@link BarChart.data}
 * @property {BarChart.legendSettings} legendSettings  Settings for position, layout and visibility of the chart legend
 * @property {number}  [decimalPlaces=4]  The data of chart will be round based on number of decimal places.
 * @property {number}  [height=null]  The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                                    By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number} [width = null]  The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string} [xAxisTitle='xAxis']  The xAxis Title
 * @property {string} [yAxisTitle='yAxis']  The yAxis Title
 * @property {string} [chartTitle = Bar Chart]  The chart Title
 * @property {string} seriesName: the bar series name will also appear in the legend
 * @property {string} [seriesColor = '#50BED7'] the color of the series ,
 * @property {string} [seriesHoverColor = '#0F789B'] the color of the series on mouse hover,
 * @property {string} [categoryTooltipCaption = 'category'] tool tip caption text displayed for the Categories
 * @property {string} [countTooltipCaption = 'count'] tool tip caption text displayed for the count 
 * @property {string} [percentageTooltipCaption = 'percentage'] tool tip caption text displayed for the percentage
 * @property {string} [classificationTooltipCaption ='classification'] tool tip caption text displayed for the classification
 * @property {boolean}  [animation=false] -Enable or disable the initial animation
 * @property {BarChart.fontSize} [fontSize]-Option to set Fontsize of label & title.
 */
/**
 * @description the values to set the legend to the prefered position or to hide it totaly
 * @typedef {object} legendSettings
 * @memberof BarChart
 * @property {boolean} [enabled = true] - show or hide the legend.
 * @property {string} [align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */
/**
 * @description The series data format for Bar Chart.
 * @typedef {Object} data
 * @memberof BarChart
 * @example
*{
*   "data":[
*     {
*         "category":"DefDefect1",
*         "classification":"critical",
*         "count":10,
*         "percentage":10
*      },
*	   {
*         "category":"DefDefect2",
*         "classification":"Major",
*         "count":20,
*         "percentage":20
*      },
*	   {
*         "category":"DefDefect3",
*         "classification":"Minor",
*         "count":30,
*         "percentage":30
*      },
*	   {
*         "category":"DefDefect4",
*         "classification":"Normal",
*         "count":40,
*         "percentage":40
*      }
*   ]
*}
*/

/**
 * @typedef {object} fontSize
 * @memberof BarChart
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */
$.fn.barChart = function (options) {
    setDefaults();
    if (options && typeof options === 'object') {
        var settings = $.extend({
            data: {},
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            chartTitle: 'Bar',
            xAxisTitle: 'xAxis',
            yAxisTitle: 'yAxis',
            seriesName: 'series',
            seriesColor: '#50BED7',
            seriesHoverColor: '#0F789B',
            categoryTooltipCaption: 'category',
            countTooltipCaption: 'count',
            percentageTooltipCaption: 'percentage',
            classificationTooltipCaption:'classification',
            legendSettings: {
                enabled: true,
                align: 'right',
                verticalAlign: 'top',
                layout: 'vertical',
                x: 0,
                y: 100
            },
            animation: false

        }, options);
    } else if (!options)
        throw 'Not valid options or parameter to the "chart" widget passed';
    var $this = this,
        _args = arguments,
        chartOption = createChartOptions(settings),
        chart = {
            init: function () {
                var containerId = $this.attr('id');
                //var _chart =
                Highcharts.chart(containerId, chartOption);
            },
            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof BarChart
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered.
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#BarChart').BarChart('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64',
            *                                           width : 1800,
            *                                           height: 600,
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        };
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function createChartOptions(settings) {
        var input = {
            categories: [],
            data: []
        },
            siemens_tooltip = $.extend({}, siemensTooltip);

        prepareData(input);
        var optionValues = {
            chart: {
                height: settings.height,
                width: settings.width,
                animation: settings.animation,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                },
                type: 'column'
            },
            title: {
                text: options.chartTitle,
                align: 'center',
                style: titleStyle
            },
            credits: {
                enabled: false
            },
            plotOptions: {
                series: {
                    animation: settings.animation,
                }
            },
            tooltip: siemens_tooltip, // siemensTooltip,
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            yAxis: {
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                labels: {
                    enabled: true,
                    style: axiesStyle,
                    formatter: function () {
                        return this.value 
                    }
                },
                title: {
                    text: settings.yAxisTitle, 
                    style: axiesTitleStyle
                },
            },
            xAxis: {
                title: {
                    text: settings.xAxisTitle,
                    style: axiesTitleStyle
                },
                labels: {
                    style: axiesStyle,

                },
                gridLineColor: siemensColors.PL_BLACK_22,
                categories: input.categories
            },
            series: prepareSeries(input)
        };
        validateLegendPositioning(optionValues.legend);
        return optionValues;
    }
    function prepareData(input) {
        $.each(settings.data.data, function (index, mydata) {
            input.categories.push(roundFloat(mydata.category));
            input.data.push(roundFloat(mydata.count));
        });
    }
    function prepareSeries(input) {
        var ser = [
            {
                name: settings.seriesName,
                data: input.data,
                color: settings.seriesColor,
                animation: settings.animation,
                states: {
                    hover: {
                        color: chartSeriesColors.Pareto.datahover
                    }
                }
            }
        ];
        return ser;
    }
    function displayTooltip(/*tooltip*/) {
        if (settings.data) {
            var msg = "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:600'>" + this.series.name + ": </span><span style='font-size:9pt;font-weight:400'>" + roundFloat(this.y, settings.decimalPlaces) + "</span></div>";
            var key = this.x;
            var oObj = settings.data.data.filter(function (obj) {
                return obj.category === key;
            });
            if (oObj[0]) {
                var caption = "<span style='font-size: 9pt; font-weight: 600'>" + settings.categoryTooltipCaption + ":</span><span style='font-size: 9pt; font-weight: 400'> " + roundFloat(oObj[0].category) + "</span><br>";
                var caption = "<span style='font-size: 9pt; font-weight: 600'>" + settings.categoryTooltipCaption + ":</span><span style='font-size: 9pt; font-weight: 400'> " + roundFloat(oObj[0].category) + "</span><br>";
                var countCaption = "<span style='font-size: 9pt; font-weight: 600'>" + settings.countTooltipCaption + ":</span><span style='font-size: 9pt; font-weight: 400'> " + roundFloat(oObj[0].count) + "</span><br>";
                var percentCaption = "<span style='font-size: 9pt; font-weight: 600'>" + settings.percentageTooltipCaption + ":</span><span style='font-size: 9pt; font-weight: 400'> " + roundFloat(oObj[0].percentage, settings.decimalPlaces) + " %</span><br>";
                var classCaption = "<span style='font-size: 9pt; font-weight: 600'>" + settings.classificationTooltipCaption + ":</span><span style='font-size: 9pt; font-weight: 400'> " + oObj[0].classification + "</span><br>";
                msg = "<div style='padding:8px 16px'>" + caption + countCaption + percentCaption + classCaption + "</div>";
            }
        }
        return msg;
    }
    function setDefaults() {
        if (typeof options === 'object') {
            siemensTooltip.formatter = displayTooltip;

            if (options.fontSize && options.fontSize.labels) {
                axiesTitleStyle.fontSize = options.fontSize.labels;
            }
            if (options.fontSize && options.fontSize.title) {
                titleStyle.fontSize = options.fontSize.title;
            }
            
        }
    }
};
/**
 * @description Provide methods for drawing a Pie chart .
 *
 * @namespace pieChart
 * */

/**
 * @description A new Pie chart can be drawn using this method.
 * @memberof pieChart
 * @function pieChart
 * @requires highstock.js
 * @requires jQuery.js
 * @param {pieChart.options} options - The chart options parameter
 * @example
 * $('#paretoChart').pieChart({
 *              data: {},
                decimalPlaces: 4,
                height: undefined,
                width: undefined,
                chartTitle: 'Pie',
                seriesName: 'series',
                categoryTooltipCaption: 'category',
                countTooltipCaption: 'count',
                percentageTooltipCaption: 'percentage'
 *          });
 */

/**
 * @memberof pieChart
 * @typedef {object} options
 * @property {pieChart.data} data  Array of defect in a defined format {@link pieChart.data}
 * @property {pieChart.legendSettings} legendSettings Settings for position, layout and visibility of the chart legend
 * @property {number}  [decimalPlaces=4]  The data of chart will be round based on number of decimal places.
 * @property {number}  [height=null]  The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                                    By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number} [width = null]  The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string} [chartTitle = Pie Chart]  The chart Title
 * @property {string} seriesName: the Pie series name will also appear in the legend
 * @property {string} [categoryTooltipCaption = 'category'] tool tip caption text displayed for the Categories
 * @property {string} [countTooltipCaption = 'count'] tool tip caption text displayed for the count 
 * @property {string} [percentageTooltipCaption = 'percentage'] tool tip caption text displayed for the percentage
 * @property {boolean}  [animation=false] -Enable or disable the initial animation
 * @property {pieChart.fontSize} [fontSize]-Option to set Fontsize of title.
 */

/**
 * @description the legend position settings
 * @typedef {object} legendSettings
 * @memberof pieChart
 * @property {boolean} [legendSettings.enabled = true] - show or hide the legend.
 * @property {string} [legendSettings.align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [legendSettings.verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [legendSettings.layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [legendSettings.x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [legendSettings.Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */

/**
 * @description The series data format for Pie  Chart.
 * @typedef {Object} data
 * @memberof pieChart
 * @example
*   data:{[
    *	    {
    *		    "category": "DefDefect5",
    *		    "count": 170,
    *		    "percentage": 14.492753623188407
    *	    },
    *	    {
    *		    "defectID": "DefDefect4",
    *		    "numberOfDefects": 145,
    *		    "percentage": 12.361466325660699
    *	    }
*       ]}
*/
/**
 * @typedef {object} fontSize
 * @memberof pieChart
 * @description Option to set Fontsize of title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @example
 *   fontSize: {
            title: 20
    }
 */

$.fn.pieChart = function (options) {
    setDefaults();
    if (options && typeof options === 'object') {
        var settings = $.extend({
            data: {},
            decimalPlaces: 4,
            height: 600,
            width: undefined,
            fontSize: {
                title: undefined
            },
            chartTitle: 'Pie',
            seriesName: 'series',
            categoryTooltipCaption: 'category',
            countTooltipCaption: 'THEcount',
            percentageTooltipCaption: 'percentage',
            legendSettings: {
                enabled: true,
                align: 'right',
                verticalAlign: 'top',
                layout: 'vertical',
                x: 0,
                y: 100
            },
            animation:false

        }, options);
    } else if (!options)
        throw 'Not valid options or parameter to the "chart" widget passed';
    var $this = this,
        _args = arguments,
        chartOption = createChartOptions(settings),
        chart = {
            init: function () {
                var containerId = $this.attr('id');
                //var _chart =
                Highcharts.chart(containerId, chartOption);
            },

            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof pieChart
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered.
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#pieChart').pieChart('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64',
            *                                           width : 1800,
            *                                           height: 600,
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        };
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function createChartOptions(settings) {
        var input = {
            data: []
        }
        prepareData(input);
        var optionValues = {
            chart: {
                plotBackgroundColor: null,
                plotBorderWidth: null,
                plotShadow: false,
                type: 'pie',
                animation: settings.animation
            },
            title: {
                text: options.chartTitle,
                align: 'center',
                style: titleStyle
            },
            credits: {
                enabled: false
            },
            tooltip: {
                pointFormat: pointToolTip()//'{series.name}: <br>{point.percentage:.1f} %<br>total: {point.total}'
            },
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: false
                    },
                    showInLegend: true,
                    animation: settings.animation
                }
            },
            series: prepareSeries(input)
        };
        validateLegendPositioning(optionValues.legend);
        return optionValues;
    }
    function prepareData(input) {
        $.each(settings.data.data, function (index, mydata) {
            var dataUnit = {
                y: undefined,
                name:''
            };
            dataUnit.name = mydata.category;
            dataUnit.y = roundFloat(mydata.count);

            input.data.push(dataUnit);
        });
    }
    function prepareSeries(input) {
        var ser = [
            {
                name: settings.seriesName,
                data: input.data,
                animation: settings.animation,
                colorByPoint: true,
                states: {
                    hover: {
                        color: chartSeriesColors.Pareto.datahover
                    }
                }
            }
        ];
        return ser;
    }
    function pointToolTip(/*Pietooltip*/) {
        
            var msg = "";
            // var tho = this;
            var caption = "<span style='font-size: 9pt; font-weight: 600'>" + settings.categoryTooltipCaption + ":</span><span style='font-size: 9pt; font-weight: 400'> {point.name} </span><br>";
        var countCaption = "<span style='font-size: 9pt; font-weight: 600'>" + settings.countTooltipCaption + ":</span><span style='font-size: 9pt; font-weight: 400'> {point.y} </span><br>";
        var percentCaption = "<span style='font-size: 9pt; font-weight: 600'>" + settings.percentageTooltipCaption + ":</span><span style='font-size: 9pt; font-weight: 400'> {point.percentage:." + settings.decimalPlaces + "f}% </span><br>";
                msg = "<div style='padding:8px 16px'>" + caption + countCaption + percentCaption +"</div>";
     
        return msg;
    }
    function setDefaults() {
        if (typeof options === 'object') {

            if (options.fontSize && options.fontSize.title) {
                titleStyle.fontSize = options.fontSize.title;
            }
        }
    }
};
/**
 * @description Provide methods for drawing a Scatter chart in 2D space.
 *
 * @namespace GPlot
 * */

/**
 * @description A new gPlot (xy) chart can be drawn by using this method.
 * @memberof GPlot
 * @function gPlot
 * @requires highstock.js
 * @requires jQuery.js
 * @param {GPlot.options} options - The chart options parameter
 * @example
 * $('#gPlot').gPlot({
 *              data: [], // An array of two arrays
 *              decimalPlaces: 4,
 *              height: 400,
 *              width: 800,
 *              chartTitle: 'G-Plot',
 *              xAxisTitle: 'xAxis',
 *              yAxisTitle: 'yAxis',
 *              series1Name: 'series1',
 *              series2Name: 'series2',
 *              series1Color: '#50BED7',
 *              series2Color: '#50BED7'                
 *          });
 */

/**
 * @memberof GPlot
 * @typedef {object} options
 * @property {GPlot.data} data - Array of two arrays. Please see format {@link GPlot.data}
 * @property {number}  [decimalPlaces=4] - The data of chart will be rounded based on number of decimal places in tooltip.
 * @property {number}  [height=null]  The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                                    By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number} [width = null]  The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string} [xAxisTitle='xAxis'] - The xAxis Title
 * @property {string} [yAxisTitle='yAxis'] - The yAxis Title
 * @property {string} [chartTitle = 'GPlot'] - The chart Title
 * @property {string} [series[n]Name = 'Series1'] - The name of the nth series. This name will be appear in the legend. For example Series2Name : 
 * @property {string} [series[n]Color = '#50BED7'] - The color of the nth series. For example Series3Color : '#FFDDRR' 
 * @property {GPlot.legendSettings} legendSettings - Settings for position, layout and visibility of the chart legend
 * @property {boolean}  [animation=false] -Enable or disable the initial animation
 * @property {boolean} [linear = false] - If true will join the scatter points with line.
 * @property {GPlot.fontSize} [fontSize]-Option to set Fontsize of label & title.
 */
/**
 * @description the legend position settings
 * @typedef {object} legendSettings
 * @memberof GPlot
 * @property {boolean} [legendSettings.enabled = true] - show or hide the legend.
 * @property {string} [legendSettings.align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [legendSettings.verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [legendSettings.layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [legendSettings.x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [legendSettings.Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */
/**
 * @typedef {object} fontSize
 * @memberof GPlot
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */
/**
 * @description The data format for the G-Plot. The data can be an array of two Arrays or simple data array. As shown in the example below. 
 * @typedef {Object} data
 * @memberof GPlot
 * @example
 * // Data can be probided in n number of inner array
 * // Arrays for n Series with 1D Data
 *  [
 * 	    [ [ 161.2] , [167.5], [159.5], [157.0], [155.8] ],--> correspond to Series 1 Data
 * 	    [ [ 160.2] , [169.5], [154.5], [157.4], [156.8] ]	-- correspond to Series 2 Data
 * 	    .
 * 	    .
 * 	    .
 * 	    .
 * 	    [ [ 160.2] , [169.5], [154.5], [157.4], [156.8] ]	-- correspond to Series n Data
 * 	]
 * OR 
 * with 2D Array elements can be provided for n series. The points correspond to x,y 
 *  [
 *          [   
 *              [161.2, 51.6], [167.5, 59.0], [159.5, 49.2], [157.0, 63.0], [155.8, 53.6]    --> correspond to Series 1 Data
 *          ],
 *          [   
 *              [170.0, 59.0], [159.1, 47.6], [166.0, 69.8], [176.2, 66.8], [160.2, 75.2]   -- correspond to Series 2 Data
 *          ],
 *          .
 *          .
 *          .
 *          .
 *          
 *          [
 *              [170.0, 59.0], [159.1, 47.6], [166.0, 69.8], [176.2, 66.8], [160.2, 75.2]   -- correspond to Series n Data
 *          ]
 *  ];
 *  
 * 
 * OR only y axis data can be provided. x-Axis will be calculated automatically. 
 * 
 * data : [161.2 , 167.5, 159.5, 157.0, 155.8] --> Series 1 data
*/
$.fn.gPlot = function (options) {
    setDefaults();
    if (options && typeof options === 'object') {
        var settings = $.extend({
            data: {},
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            chartTitle: 'GPlot',
            xAxisTitle: 'xAxis',
            yAxisTitle: 'yAxis',
            series1Name: 'series1',
            series2Name: 'series2',
            series1Color: '#50BED7',
            series2Color: '#f64242',
            legendSettings: {
                enabled: true,
                align: 'right',
                verticalAlign: 'top',
                layout: 'vertical',
                x: 0,
                y: 100
            },
            animation: false,
            linear : false
        }, options);
    } else if (!options)
        throw 'Not valid options or parameter to the "chart" widget passed';
    var $this = this,
        _args = arguments,
        chartOption = createChartOptions(settings),
        chart = {
            init: function () {
                var containerId = $this.attr('id');
                var _chart = Highcharts.chart(containerId, chartOption);

                $this.data('chartApi', _chart); // required to call methods 
                $this.data('settings', settings);
            },
            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof GPlot
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered.
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#gPlot').gPlot('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64',
            *                                           width : 1800,
            *                                           height: 600,
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        };
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function createChartOptions(settings) {
        var input = { },
            siemens_tooltip = $.extend({}, siemensTooltip),
            lineWidth = settings.linear === true ? 2 : 0;

        prepareData(input);

        var optionValues = {
            chart: {
                height: settings.height,
                width: settings.width,
                animation: settings.animation,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                },
                // zoomType: 'xy',
                type: 'scatter'
            },
            title: {
                text: settings.chartTitle,
                align: 'center',
                style: titleStyle
            },
            credits: {
                enabled: false
            },
            tooltip: siemens_tooltip, // siemensTooltip,
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            yAxis: {
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                labels: {
                    style: axiesStyle,
                },
                title: {
                    text: settings.yAxisTitle,
                    style: axiesTitleStyle
                },
            },
            xAxis: {
                title: {
                    text: settings.xAxisTitle,
                    style: axiesTitleStyle
                },
                labels: {
                    style: axiesStyle,
                },
                gridLineColor: siemensColors.PL_BLACK_22 //,
                // categories: input.categories
            },
            plotOptions: {
                series: {
                    animation: settings.animation,
                },
                scatter: {
                    lineWidth: lineWidth               
                },
            },
            series: prepareSeries(input)
        };
        validateLegendPositioning(optionValues.legend);
        return optionValues;
    }
    function prepareData(input) {
        // Data Formats 
        // 2D Array
        // [  [[],[]] , [[],[]] ]
        // 1D Array
        // 	[  [[161.2], [167.5], [159.5], [157.0], [155.8]], [[160.2], [169.5], [154.5], [157.4], [156.8]]  ]
        // [161.2 , 167.5, 159.5, 157.0, 155.8] 

        if (settings.data) {
            if (typeof settings.data[0] === 'number') { // If 1D Array
                input['series1'] = [];
                $.each(settings.data, function (i, d) {
                    // input.series1.push(d);
                    input['series1'].push(d);
                });
            }
            else if (typeof settings.data[0] === 'object' && typeof settings.data[0][0] === 'object') {
                
                $.each(settings.data, function (i, d) {
                    input['series' + (i+1)] = [];
                    $.each(d, function (j, data) {
                        input['series' + (i+1)].push(data);
                    });                    
                });

                //$.each(settings.data[0], function (i, d) {

                //    input.series1.push(d);
                //});
            }

            //if (typeof settings.data[1] === 'number') {
            //    $.each(settings.data, function (i, d) {
            //        input.series2.push(d);
            //    });
            //}
            //else if (typeof settings.data[1] === 'object' && typeof settings.data[1][0] === 'object') {
            //    $.each(settings.data[1], function (i, d) {
            //        input.series2.push(d);
            //    });
            //}
        }
    }
    function prepareSeries(input) {
        var ser = [];
        var index = 0;
        $.each(input, function (i, d) {
            index++;
            var obj = {
                name: settings['series'+ index +'Name'],
                data: input['series'+ index ],
                animation: settings.animation,
                color: settings['series'+ index + 'Color']                
            }
            ser.push(obj);
        });
        
        //var ser = [
        //    {
        //        name: settings.series1Name,
        //        data: input.series1,
        //        animation: settings.animation,
        //        color: settings.series1Color//,
        //        //states: {
        //        //    hover: {
        //        //        color: chartSeriesColors.Pareto.datahover
        //        //    }
        //        //}
        //    },
        //    {
        //        name: settings.series2Name,
        //        data: input.series2,
        //        animation: settings.animation,
        //        color: settings.series2Color//,
        //        //states: {
        //        //    hover: {
        //        //        color: chartSeriesColors.Pareto.datahover
        //        //    }
        //        //}
        //    }
        //];
        return ser;
    }
    function displayTooltip(tooltip) {

        var msg = "<div style='padding:8px 16px'>" +
            "<span style = 'font-size:9pt;font-weight:600' >" + settings.xAxisTitle + ": </span> <span style='font-size:9pt;font-weight:400'>" + roundFloat(this.x, settings.decimalPlaces) + "</span><br>" +
            "<span style = 'font-size:9pt;font-weight:600' >" + settings.yAxisTitle + ": </span> <span style='font-size:9pt;font-weight:400'>" + roundFloat(this.y, settings.decimalPlaces) + "</span>" +
            "</div > ";

        return msg;
    }
    function setDefaults() {
        if (typeof options === 'object') {
            siemensTooltip.formatter = displayTooltip;

            if (options.fontSize && options.fontSize.labels) {
                axiesTitleStyle.fontSize = options.fontSize.labels;
            }
            if (options.fontSize && options.fontSize.title) {
                titleStyle.fontSize = options.fontSize.title;
            }
        }
    }
};
/**
 * @description Provide methods for drawing a Scatter chart in 3D space.
 *
 * @namespace GPlot3D
 */

/**
 * @description A new gPlot in 3D space can be drawn by using this method.
 * @memberof GPlot3D
 * @function gPlot3d
 * @requires highstock.js
 * @requires highstock-3d.js
 * @requires jQuery.js
 * @param {GPlot3D.options} options - The chart options parameter
 * @example
 * $('#gPlot3d').gPlot3d({
 *              data: [], // An array data in format [ [ 1, 6, 5 ], [ 8, 7, 9 ]]
 *              decimalPlaces: 4,
 *              height: 400,
 *              width: 800,
 *              chartTitle: 'G-Plot-3D',
 *              xAxisTitle: 'xAxis',
 *              yAxisTitle: 'yAxis',
 *              zAxisTitle: 'zAxis',
 *              seriesName: 'series',
 *              series1Color: '#50BED7',
 *              series2Color: '#50BED7'                
 *          });
 */
/**
 * @description the legend position settings
 * @typedef {object} legendSettings
 * @memberof GPlot3D
 * @property {boolean} [legendSettings.enabled = true] - show or hide the legend.
 * @property {string} [legendSettings.align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [legendSettings.verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [legendSettings.layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [legendSettings.x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [legendSettings.Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 */

/**
 * @memberof GPlot3D
 * @typedef {object} options
 * @property {GPlot3D.data} data - Array of array with 3 object. Please see format {@link GPlot3D.data}
 * @property {GPlot3D.legendSettings} legendSettings - Settings for position, layout and visibility of the chart legend
 * @property {number}  [decimalPlaces=4] - The data of chart will be rounded based on number of decimal places in tooltip.
 * @property {number}  [height=null]  The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                                    By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number} [width = null]  The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string} [xAxisTitle='xAxis'] - The xAxis Title
 * @property {string} [yAxisTitle='yAxis'] - The yAxis Title
 * @property {string} [zAxisTitle='zAxis'] - The yAxis Title
 * @property {string} [chartTitle = 'GPlot3D'] - The chart Title 
 * @property {boolean}  [animation=false] -Enable or disable the initial animation
 * @property {GPlot3D.fontSize} [fontSize]-Option to set Fontsize of label & title.
 */
/**
 * @description The data format for the G-Plot 3D. The data can be an array arrays with 3 objects. As shown in the example below. 
 * @typedef {object} data
 * @memberof GPlot3D
 * @example
 * [
 *  [ 1, 6, 5 ],  [ 8, 7, 9 ],  [ 1, 3, 4 ],  [ 4, 6, 8 ],  [ 5, 7, 7 ],  [ 6, 9, 6 ],  [ 7, 0, 5 ],  [ 2, 3, 3 ],  [ 3, 9, 8 ],
 *  [ 3, 6, 5 ],  [ 4, 9, 4 ],  [ 2, 3, 3 ],  [ 6, 9, 9 ],  [ 0, 7, 0 ],  [ 7, 7, 9 ],  [ 7, 2, 9 ],  [ 0, 6, 2 ],  [ 4, 6, 7 ],
 *  [ 3, 7, 7 ],  [ 0, 1, 7 ],  [ 2, 8, 6 ],  [ 2, 3, 7 ],  [ 6, 4, 8 ],  [ 3, 5, 9 ],  [ 7, 9, 5 ],  [ 3, 1, 7 ],  [ 4, 4, 2 ],
 *  [ 3, 6, 2 ],  [ 3, 1, 6 ],  [ 6, 8, 5 ],  [ 6, 6, 7 ],  [ 4, 1, 1 ],  [ 7, 2, 7 ],  [ 7, 7, 0 ],  [ 8, 8, 9 ],  [ 9, 4, 1 ],
 *  [ 8, 3, 4 ],  [ 9, 8, 9 ],  [ 3, 5, 3 ],  [ 0, 2, 4 ],  [ 6, 0, 2 ],  [ 2, 1, 3 ],  [ 5, 8, 9 ],  [ 2, 1, 1 ],  [ 9, 7, 6 ],
 *  [ 3, 0, 2 ],  [ 9, 9, 0 ],  [ 3, 4, 8 ],  [ 2, 6, 1 ],  [ 8, 9, 2 ],  [ 7, 6, 5 ],  [ 6, 3, 1 ],  [ 9, 3, 1 ],  [ 8, 9, 3 ],
 *  [ 9, 1, 0 ],  [ 3, 8, 7 ],  [ 8, 0, 0 ],  [ 4, 9, 7 ],  [ 8, 6, 2 ],  [ 4, 3, 0 ],  [ 2, 3, 5 ],  [ 9, 1, 4 ],  [ 1, 1, 4 ],
 *  [ 6, 0, 2 ],  [ 6, 1, 6 ],  [ 3, 8, 8 ],  [ 8, 8, 7 ],  [ 5, 5, 0 ],  [ 3, 9, 6 ],  [ 5, 4, 3 ],  [ 6, 8, 3 ],  [ 0, 1, 5 ],
 *  [ 6, 7, 3 ],  [ 8, 3, 2 ],  [ 3, 8, 3 ],  [ 2, 1, 6 ],  [ 4, 6, 7 ],  [ 8, 9, 9 ],  [ 5, 4, 2 ],  [ 6, 1, 3 ],  [ 6, 9, 5 ],
 *  [ 4, 8, 2 ],  [ 9, 7, 4 ],  [ 5, 4, 2 ],  [ 9, 6, 1 ],  [ 2, 7, 3 ],  [ 4, 5, 4 ],  [ 6, 8, 1 ],  [ 3, 4, 0 ],  [ 2, 2, 6 ],
 *  [ 5, 1, 2 ],  [ 9, 9, 7 ],  [ 6, 9, 9 ],  [ 8, 4, 3 ],  [ 4, 1, 7 ],  [ 6, 2, 5 ],  [ 0, 4, 9 ],  [ 3, 5, 9 ],  [ 6, 9, 1 ]
 *  ]
*/
/**
 * @typedef {object} fontSize
 * @memberof GPlot3D
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */
$.fn.gPlot3d = function (options) {
    setDefaults();
    if (options && typeof options === 'object') {
        var settings = $.extend({
            data: {},
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            chartTitle: 'GPlot3D',
            xAxisTitle: 'xAxis',
            yAxisTitle: 'yAxis',
            zAxisTitle: 'zAxis',
            seriesName: 'series',            
            series1Color: '#50BED7',
            series2Color: '#f64242',
            legendSettings: {
                enabled: true,
                align: 'right',
                verticalAlign: 'top',
                layout: 'vertical',
                x: 0,
                y: 100
            },
            animation: false
        }, options);
    } else if (!options)
        throw 'Not valid options or parameter to the "chart" widget passed';
    var $this = this,
        _args = arguments,        
        chartOption = createChartOptions(settings),
        chart = {
            init: function () {
                var containerId = $this.attr('id'),
                chart3d = Highcharts.chart(containerId, chartOption);
                bindDragEvent(chart3d);
            },
            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof GPlot3D
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - The width of chart. By default will take the chart container width where chart is rendered.
            * @param  {number} [options.height] - The height of chart. By default will take the chart container height where chart is rendered.
            * @param  {Function} options.success - Callback when chart is rendered.
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            * $('#gPlot3d').gPlot3d('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64',
            *                                           width : 1800,
            *                                           height: 600,
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            }
        };
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    function createChartOptions(settings) {
        var input = {
            series1: [],
            series2: []
        },
            siemens_tooltip = $.extend({}, siemensTooltip);

        prepareData(input);

        var optionValues = {
            chart: {
                height: settings.height,
                width: settings.width,
                animation: settings.animation,
                style: {
                    fontFamily: 'Segoe UI,Open Sans,Arial,Helvetica,sans-serif'
                },
                // zoomType: 'xy',
                type: 'scatter3d',
                options3d: {
                    enabled: true,
                    alpha: 20,
                    beta: 30,
                    depth: 200,
                    viewDistance: 10,
                    frame: {
                        bottom: { size: 1, color: 'rgba(0,0,0,0.02)' },
                        back: { size: 1, color: 'rgba(0,0,0,0.04)' },
                        side: { size: 1, color: 'rgba(0,0,0,0.06)' }
                    }
                }
            },
            title: {
                text: settings.chartTitle,
                align: 'center',
                style: titleStyle
            },
            plotOptions: {
                scatter: {
                    width: 10,
                    height: 10,
                    depth: 10,
                    animation: settings.animation,
                }
            },
            credits: {
                enabled: false
            },
            tooltip: siemens_tooltip, // siemensTooltip,
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            yAxis: {
                lineColor: siemensColors.PL_BLACK_22,
                lineWidth: 2,
                labels: {
                    style: axiesStyle,
                },
                title: {
                    text: settings.yAxisTitle,
                    style: axiesTitleStyle
                },
            },
            xAxis: {
                title: {
                    text: settings.xAxisTitle,
                    style: axiesTitleStyle
                },
                labels: {
                    style: axiesStyle,
                },
                gridLineColor: siemensColors.PL_BLACK_22 //,
                // categories: input.categories
            },
            zAxis: {
                //min: 0,
                //max: 10,
                showFirstLabel: false,
                title: {
                    text: settings.zAxisTitle,
                    style: axiesTitleStyle
                },
                labels: {
                    style: axiesStyle,
                },
            },
            series: prepareSeries(input)
        };
        validateLegendPositioning(optionValues.legend);
        return optionValues;
    }
    function prepareData(input) {
        // {[], []} , [[],[]]

        if (settings.data) {
            return input.series1 = settings.data;
        }
        //if (settings.data) {
        //    if (typeof settings.data[0] === 'number') {
        //        $.each(settings.data, function (i, d) {
        //            input.series1.push(d);
        //        });
        //    }
        //    else if (typeof settings.data[0] === 'object' && typeof settings.data[0][0] === 'object') {
        //        $.each(settings.data[0], function (i, d) {
        //            input.series1.push(d);
        //        });
        //    }

        //    if (typeof settings.data[1] === 'number') {
        //        $.each(settings.data, function (i, d) {
        //            input.series2.push(d);
        //        });
        //    }
        //    else if (typeof settings.data[1] === 'object' && typeof settings.data[1][0] === 'object') {
        //        $.each(settings.data[1], function (i, d) {
        //            input.series2.push(d);
        //        });
        //    }
        //}
    }
    function prepareSeries(input) {
        var ser = [
            {
                name: settings.seriesName,
                data: input.series1,
                animation: settings.animation,
                colorByPoint: false// ,    
                // color: settings.series1Color//,
                //states: {
                //    hover: {
                //        color: chartSeriesColors.Pareto.datahover
                //    }
                //}
            }           
        ];
        return ser;
    }
    function displayTooltip() {

        var msg = "<div style='padding:8px 16px'>" +
            "<span style = 'font-size:9pt;font-weight:600' >" + settings.xAxisTitle + ": </span> <span style='font-size:9pt;font-weight:400'>" + roundFloat(this.point.x, settings.decimalPlaces) + "</span><br>" +
            "<span style = 'font-size:9pt;font-weight:600' >" + settings.yAxisTitle + ": </span> <span style='font-size:9pt;font-weight:400'>" + roundFloat(this.point.y, settings.decimalPlaces) + "</span><br>" +
            "<span style = 'font-size:9pt;font-weight:600' >" + settings.zAxisTitle + ": </span> <span style='font-size:9pt;font-weight:400'>" + roundFloat(this.point.z, settings.decimalPlaces) + "</span>" +
            "</div > ";

        return msg;
    }
    function setDefaults() {
        if (typeof options === 'object') {
            siemensTooltip.formatter = displayTooltip;

            if (options.fontSize && options.fontSize.labels) {
                axiesTitleStyle.fontSize = options.fontSize.labels;
            }
            if (options.fontSize && options.fontSize.title) {
                titleStyle.fontSize = options.fontSize.title;
            }

        }
    }

    // Add mouse and touch events for rotation
    function bindDragEvent(chart) {
        (function (H) {
            function dragStart(eStart) {
                eStart = chart.pointer.normalize(eStart);

                var posX = eStart.chartX,
                    posY = eStart.chartY,
                    alpha = chart.options.chart.options3d.alpha,
                    beta = chart.options.chart.options3d.beta,
                    sensitivity = 5,  // lower is more sensitive
                    handlers = [];

                function drag(e) {
                    // Get e.chartX and e.chartY
                    e = chart.pointer.normalize(e);

                    chart.update({
                        chart: {
                            options3d: {
                                alpha: alpha + (e.chartY - posY) / sensitivity,
                                beta: beta + (posX - e.chartX) / sensitivity
                            }
                        }
                    }, undefined, undefined, false);
                }

                function unbindAll() {
                    handlers.forEach(function (unbind) {
                        if (unbind) {
                            unbind();
                        }
                    });
                    handlers.length = 0;
                }

                handlers.push(H.addEvent(document, 'mousemove', drag));
                handlers.push(H.addEvent(document, 'touchmove', drag));


                handlers.push(H.addEvent(document, 'mouseup', unbindAll));
                handlers.push(H.addEvent(document, 'touchend', unbindAll));
            }
            H.addEvent(chart.container, 'mousedown', dragStart);
            H.addEvent(chart.container, 'touchstart', dragStart);
        }(Highcharts));
    }
};
/**
 * @description Provide methods for drawing universal Stacked Pareto chart .
 *
 * @namespace StackedPareto
 * */

/**
 * @description Draw a  universal Stacked Pareto chart using the provided data from the options parameter
 * @memberof StackedPareto
 * @function stackedPareto
 * @requires highstock.js
 * @requires jQuery.js
 * @param {StackedPareto.options} options - The chart options parameter
 * @example
 *   $('#stackedPareto').stackedPareto({
            data: data,
            decimalPlaces: 4,
            height: 600,
            width: undefined,
            yAxisTitle: 'Defects Distribution',
            chartTitle: 'Stacked Column Pareto',
            animation: false,
            legendSettings: {
                enabled: true,
                align: 'center',
                verticalAlign: 'top',
                layout: 'horizontal',
                x: 0,
                y: 0
            }
        });
*/

/**
 * @description the legend position settings
 * @memberof StackedPareto
 * @typedef {object} legendSettings 
 * @property {boolean} [enabled = true] - show or hide the legend.
 * @property {string} [align = right] - The horizontal alignment of the legend box within the chart area. Valid values are left, center and right.
 * @property {string} [verticalAlign = top] - The vertical alignment of the legend box. Can be one of top, middle or bottom.
 * @property {string} [layout = vertical] - The layout of the legend items. Can be one of horizontal or vertical or proximate. When proximate, the legend items will be placed as close as possible to the graphs they're representing, except in inverted charts or when the legend position doesn't allow it.
 * @property {number} [x = 0] - The x offset of the legend relative to its horizontal alignment align within chart.spacingLeft and chart.spacingRight. Negative x moves it to the left, positive x moves it to the right.
 * @property {number} [Y = 0] - The vertical offset of the legend relative to it's vertical alignment verticalAlign within chart.spacingTop and chart.spacingBottom. Negative y moves it up, positive y moves it down.
 * @property {StackedPareto.onChartLoaded} [onChartLoaded=null] - A callback can be register which will be invoked after chart is data is loaded and it is going to render.
 * @property {string}  [xAxisTitle] - The xAxis Title
 */

/**
 * @memberof StackedPareto
 * @typedef {object} options
 * @property {StackedPareto.data} data - chart data Object {@link StackedPareto.data}
 * @property {string} [locale=en] - User locale
 * @property {StackedPareto.legendSettings} legendSettings - Settings for position, layout and visibility of the chart legend
 * @property {string} chartTitle - The chart's main title.
 * @property {number} [decimalPlaces = 4] - The number of digits to appear after the decimal point.
 * @property {number} [height=null] - The chart height in pixel unit or as percentage %. An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.<br>
 *                               By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
 * @property {number} [width = null] - The chart width in pixel. This is an explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
 * @property {string} [yAxisTitle=null] - The x-Axis label displayed underneath the x-axis.
 * @property {boolean}  [animation=false] - Enable or disable the initial animation
 * @property {StackedPareto.options3d} [options3d]-Options to render charts in 3 dimensions. This feature requires highcharts-3d.js
 * @property {StackedPareto.fontSize} [fontSize]-Option to set Fontsize of label & title.
*/

/**
 * @typedef {object} options3d
 * @memberof StackedPareto
 * @description -Options to render charts in 3 dimensions. This feature requires highcharts-3d.js
 * @property {number} [alpha=0]- One of the two rotation angles for the chart.
 * @property {number} [beta=0]- One of the two rotation angles for the chart.
 * @property {number} [depth=100]- The total depth of the chart.
 * @property {boolean} [enabled=false]- Wether to render the chart using the 3D functionality.
 * @property {number} [viewDistance=25]- Defines the distance the viewer is standing in front of the chart, this setting is important to calculate the perspective effect in column charts.
 * @example
 *   options3d: {
 *       enabled: false,
 *       alpha: 0,
 *       beta: 0,
 *       depth: 20,
 *       viewDistance: 15
 *   }
 */

/**
 * @typedef {object} fontSize
 * @memberof StackedPareto
 * @description Option to set Fontsize of label & title.
 * @property {number} [title=undefined]- Fontsize for title.
 * @property {number} [labels=undefined]- Fontsize for labels.
 * @example
 *   fontSize: {
            title: 20,
            labels: 15
    }
 */

/**
* @description The series data format for Stacked Pareto Chart.
* @memberof StackedPareto
* @typedef {Object} data
* @example
*   data:{
*	"categories": [
*		"Defect1",
*		"Defect2",
*		"Defect3",
*		"Defect4",
*		"Defect5"
*	],
*	"series": [
*		{
*			"name": "Major",
*			"data": [
*				5,
*				3,
*				4,
*				7,
*				2
*			]
*		},
*		{
*			"name": "Minor",
*			"data": [
*				2,
*				2,
*				3,
*				2,
*				1
*			]
*		},
*		{
*			"name": "Serious",
*			"data": [
*				3,
*				4,
*				4,
*				2,
*				5
*			]
*		}
*	]
}*
*/
$.fn.stackedPareto = function (options) {
    var settings = {};
    setDefaults();
    if (options && typeof options === 'object') {
        settings = $.extend({
            data: {},
            decimalPlaces: 4,
            height: undefined,
            width: undefined,
            fontSize: {
                title: undefined,
                labels: undefined
            },
            yAxisTitle: '',
            onChartLoaded: '',
            chartTitle: 'Stacked Pareto',
            xAxisTitle: undefined,
            animation: false,
            options3d: {
                enabled: false,
                alpha: 0,
                beta: 0,
                depth: 20,
                viewDistance: 15
            },
            legendSettings: {
                enabled: true,
                align: 'right',
                verticalAlign: 'top',
                layout: 'vertical',
                x: 0,
                y: 100
            }
        }, options);
    } else if (typeof options === 'string') { // Method call
        settings = $(this).data('settings') || {};  // restore the settings object from prev settings
    } else if (!options)
        throw 'Not valid options or parameter to the "chart" widget passed';

    var $this = this,
        _args = arguments,
        chartOption = createChartOptions(settings),
        chart = {
            init: function () {
                var containerId = $this.attr('id');
                var _chart = Highcharts.chart(containerId, chartOption);
                $this.data('chartApi', _chart);
                
                $this.data('settings', settings);
            },

            /**
            * @description - Export the chart as base 64 image. Only support the compatible HTML5 browsers. For IE, it requires canvg.js from v3.0.8 to onwards.
            * @memberof StackedPareto
            * @function createBase64Image
            * @param {object} options - An object with specified properties
            * @param  {string} options.containerId - The id of an html input element e.g. base64Image when a tag is defined like this <input type='hidden' id='base64Image'/>
            * @param  {number} [options.width] - An explicit width for the chart. By default (when null) the width is calculated from the offset width of the containing element.
            * @param  {number} [options.height] - An explicit height for the chart. If a number, the height is given in pixels. If given a percentage string (for example '56%'), the height is given as the percentage of the actual chart width. This allows for preserving the aspect ratio across responsive sizes.By default (when null) the height is calculated from the offset height of the containing element, or 400 pixels if the containing element's height is 0.
            * @param  {Function} options.success - Callback when chart is rendered. 
            * @param {Function} [options.error] - Callback to get error information.
            * @requires exporting.js
            * @requires offline-exporting.js
            * @requires canvg.js - For Internet explorer only
            * @example
            *  $('#stackedPareto').stackedPareto('createBase64Image',
            *                                       {
            *                                           containerId : 'sImageBase64', 
            *                                           width : 1800, 
            *                                           height: 600, 
            *                                           success: function(){
            *                                                               console.log(document.getElementById('sImageBase64').value);
            *                                           },
            *                                           error: function(e) {
            *                                                               console.log(e);
            *                                           }
            *                                       });
            */
            createBase64Image: function (options) {
                var chartApi = $this.data('chartApi');
                createBase64Image(chartApi, options);
            },
            /**
             * @description set 3D render status  to render the chart using the 3D functionality or not.
             * @memberof StackedPareto
             * @function toggle3D
             * @requires highcharts-3d.js
             * @param {boolean} state -enable/disable rendering the chart using the 3D functionality or not.
             * @example
             * $('#stackedPareto').stackedPareto('toggle3D',true);
             */
            toggle3D: function (state) {
                var chartApi = $this.data('chartApi');
                chartApi.update({ chart: { options3d: { enabled: state } } }, false);
                chartApi.redraw();
            }
        };
    return this.each(function () {
        if (chart[options]) {
            return chart[options]
                (_args[1], _args[2]);
        } else if (typeof options === 'object' || !options) {
            chart.init();
        }
    });

    /**
    * @description The callback which is called when a chart is drawn. 
    * @memberof StackedPareto
    * @callback onChartLoaded
    * @param {object} e - The event object with chart API object
    */
    function onChartLoaded(e) {
        if (typeof settings.onChartLoaded === 'function') { // if callback is defined
            settings.onChartLoaded(e);
        }
    }

    

    function createChartOptions(settings) {
        var siemens_tooltip = $.extend({}, siemensTooltip);
        var optionValues = {
            chart: {
                height: settings.height,
                width: settings.width,
                type: 'column',
                events: {
                    load: onChartLoaded
                },
                options3d: settings.options3d,
            },

            title: {
                text: options.chartTitle,
                align: 'center',
                style: titleStyle
            },
            credits: {
                enabled: false
            },
            xAxis: {
                categories: settings.data.categories,
                title: {
                    text: settings.xAxisTitle,
                    style: axiesTitleStyle
                }
            },
            yAxis: {
                min: 0,
                title: {
                    text: settings.yAxisTitle,
                    style: axiesTitleStyle
                },
                stackLabels: {
                    enabled: true,
                    formatter: function () {
                        return roundFloat(this.total, settings.decimalPlaces);
                    }
                }
            },
            tooltip: siemens_tooltip,
            legend: {
                enabled: settings.legendSettings.enabled,
                align: settings.legendSettings.align,
                verticalAlign: settings.legendSettings.verticalAlign,
                layout: settings.legendSettings.layout,
                x: settings.legendSettings.x,
                y: settings.legendSettings.y,
                itemStyle: axiesTitleStyle
            },
            animation: settings.animation,
            plotOptions: {
                column: {
                    stacking: 'normal',
                    dataLabels: {
                        enabled: true,
                        formatter:StackedParetoDataLabelsFormatter
                    },
                    animation: settings.animation
                }
            },
            series: prepareSeries()
        };
        validateLegendPositioning(optionValues.legend);
        return optionValues;
    }

    function prepareSeries() {
        var series = [];
        // var colorData = Object.values(chartSeriesColors.MultiVisDefect);
        var colorData = $.map(chartSeriesColors.MultiVisDefect, function (value, key) { return value; }) || [];
        $.each(settings.data.series, function (index, seriesObj) {
            var obj;
            obj = {
                'name': seriesObj.name,
                'data': seriesObj.data.map(function (elem) { return roundFloat(elem)}), //roundFloat(seriesObj.data),
                'color': colorData[index]
            }
            series.push(obj)

        });
        return series;

    }
    function displayTooltip(/*stackedParetoTooltip*/) {
        return  "<div style='padding:8px 16px'><span style='font-size:9pt;font-weight:600'>" + this.series.name + ": </span><span style='font-size:9pt;font-weight:400'>" + roundFloat(this.y, settings.decimalPlaces) + "</span></div>";
    }
    function StackedParetoDataLabelsFormatter() {
        if (this.y)
            return this.series.name + ": " + roundFloat(this.y, settings.decimalPlaces);
    }
    function setDefaults() {
        if (typeof options === 'object') {
            siemensTooltip.formatter = displayTooltip;

            if (options.fontSize && options.fontSize.labels) {
                axiesTitleStyle.fontSize = options.fontSize.labels;
            }
            if (options.fontSize && options.fontSize.title) {
                titleStyle.fontSize = options.fontSize.title;
            }
        }
    }
}
//Note: should not be referenced directly in HTML file. Will be used by bundler.js

function setDefaultLocaleText() {
    if (Highcharts /* && settings.locale === 'en'*/) {
        // Highcharts.uilocale is in init() initialized
        Highcharts.uiLocale.en = {
            "UCL": "Upper CL",
            "LCL": "Lower CL",
            "UWL": "Upper WL",
            "LWL": "Lower WL",
            "UTL": "Upper TL",
            "LTL": "Lower TL",
            "Chi2Test": "Chi² Test",
            "Eliminated": "Sample eliminated",
            "EliminatedByCalculation": "Sample is automatically eliminated",
            "USL": "Upper Specification Limit",
            "LSL": "Lower Specification Limit",
            "measurement": "Measurement",
            "UCL_1": "Upper CL (CC1)",
            "LCL_1": "Lower CL (CC1)",
            "UWL_1": "Upper WL (CC1)",
            "LWL_1": "Lower WL (CC1)",
            "Specification": "Specification",
            "TrendUp_1": "Trend up (CC1)",
            "TrendDown_1": "Trend down (CC1)",
            "RunHigh_1": "Run up (CC1)",
            "RunLow_1": "Run down (CC1)",
            "MiddleThird": "Middlethird",
            "ProcessViolation_1": "Process violation in CC1",
            "Zone": "Generic Zone",
            "ZoneRule01": "Zone Rule #1",
            "ZoneRule02": "Zone Rule #2",
            "ZoneRule03": "Zone Rule #3",
            "ZoneRule04": "Zone Rule #4",
            "ZoneRule05": "Zone Rule #5",
            "ZoneRule06": "Zone Rule #6",
            "ZoneRule07": "Zone Rule #7",
            "ZoneRule08": "Zone Rule #8",
            "ZoneRule09": "Zone Rule #9",
            "ZoneRule10": "Zone Rule #10",
            "ZoneRule11": "Zone Rule #11",
            "AboveZone": "Above Zone",
            "BelowZone": "Below Zone",
            "Changed": "Values have been changed",
            "Outlier": "Sample contains outlier",
            "UCL_2": "Upper CL (CC2)",
            "LCL_2": "Lower CL (CC2)",
            "UWL_2": "Upper WL (CC2)",
            "LWL_2": "Lower WL (CC2)",
            "TrendUp_2": "Trend up (CC2)",
            "TrendDown_2": "Trend down (CC2)",
            "RunHigh_2": "Run up (CC2)",
            "RunLow_2": "Run down (CC2)",
            "ProcessViolation_2": "Process violation in CC2",
            "ToolChanged": "Tool change",
            "Hist_tt_Count": "Count",
            "Hist_ND": "d",
            "Hist_currentLowerToleranceLimitAbs": "LT",
            "Hist_currentUpperToleranceLimitAbs": "UT",
            "Hist_Xbb": "xbb",
            "Hist_ChartTitle": "HISTOGRAM",
            "Hist_yAxisTitle": "%",
            "Hist_BellCurve": "Bell Curve",
            "SVC_xAxisTitle": "Sort number",
            "SVC_yAxisTitle": "Measurements",
            "AdditionalMeasurement": "add. Measurement",
            "xbp_1": "Process Mean Value (CC1)",
            "xbp_2": "Process Mean Value (CC2)",
            "UTL_1": "Upper TL (CC1)",
            "UTL_2": "Upper TL (CC2)",
            "LTL_1": "Lower TL (CC1)",
            "LTL_2": "Lower TL (CC2)",
            "decimalDelimiter": ".",
            "NonConformanceRate": "Non Conformance Rate",
            "DefectiveParts": "Defective Parts",
            "processMeanValue": "process Mean Value",
            "localMeanValue": "xbb",
            "HIST_Confidance_Interval": "s^ND",
            "usl_sv": "Upper tolerance violation",
            "lsl_sv": "Lower tolerance violation",
            "nominalValue": "Nominal Value",
            "lowerMiddlethird": "Lower Middlethird",
            "upperMiddlethird": "Upper Middlethird",
            "svc": "Single Value",
            "xb": "xb",
            "mxb": "mxb",
            "ewma": "ewma",
            "mr": "mR",
            "ms": "ms",
            "s": "s",
            "r": "R",
            "x": "x",
            "med": "Median",
            "measurements": "Measurements",
            "firstChoiceUpperLimit": "Tolerance 1.quality",
            "tt_firstChoiceUpperLimit": "First quality upper tol.",
            "tt_firstChoiceLowerLimit": "First quality lower tol.",
            "secondChoiceUpperLimit": "Tolerance 2.quality",
            "tt_secondChoiceUpperLimit": "Second quality upper tol.",
            "tt_secondChoiceLowerLimit": "Second quality lower tol.",
            "thirdChoiceUpperLimit": "Tolerance 3.quality",
            "tt_thirdChoiceUpperLimit": "Third quality upper tol.",
            "tt_thirdChoiceLowerLimit": "Third quality lower tol.",
            "centerOfTolerances": "Tol. center",
            "averageOfAllValues": "xq",
            "oneThirdOfControlLimits": "1/3 control limit",
            "tt_oneThirdOfControlLimitsUpper": "1/3 control limit upper",
            "tt_oneThirdOfControlLimitsLower": "1/3 control limit lower",
            "twoThirdsOfControlLimits": "2/3 control limit",
            "tt_twoThirdsOfControlLimitsUpper": "2/3 control limit upper",
            "tt_twoThirdsOfControlLimitsLower": "2/3 control limit lower",
            "tt_NominalValue": "Nominal Value",
            "x_axis_subgroups": "Subgroups",
            "xb_chart": "XB Chart",
            "xb_s_chart": "xb/s Chart",
            "mxb_ms_chart": "mxb/mS Chart",
            "xb_r_chart": "xb/r Chart",
            "mxb_mr_chart": "mxb/mR Chart",
            "x_ms_chart": "x/ms Chart",
            "x_mr_chart": "x/mr Chart",
            "s_chart": "S Chart",
            "r_chart": "R Chart",
            "svc_chart": "Single Value Chart",
            "pNet_chart": "Probability Plot",
            "pNet_xAxisTitle": "Measurement",
            "pNet_yAxisTitle": "%",
            "ms_chart": "S Chart",
            "mr_chart": "R Chart",
            "med_chart": "Median Chart",
            "med_r_chart": "Median/R Chart",
            "bPlot_low": "Min",
            "bPlot_high": "Max",
            "bPlot_median": "Median",
            "bPlot_q1": "Q1",
            "bPlot_q3": "Q3",
            "bPlot_series": "Boxplot",
            "eliminated": "Eliminated",
            "Defect_Pareto_xAxis": "Defect",
            "Defect_Pareto_yAxis": "Count",
            "Pareto_chart_title": "Defect Pareto",
            "Defect_Pareto_percent_Tooltip_Caption": 'Percentage',
            "Defect_Pareto_count_Tooltip_Caption": 'Count',
            "Defect_Pareto_class_Tooltip_Caption": "Clasification",
            "Defect_Pareto_ID_Tooltip_Caption": "Defect ID",
            "cuSum": "S",
            "cuSumLow": "SL",
            "CUSumHigh": "SH",
            "CUSumUCL": "UCL",
            "CUSumLCL": "LCL",
            "CUSumCL": "CL",
            "CUSum_chart": "Cumulative Sum Chart",
            "cuCount_chart": "Cumulative Count Chart",
            "cuCount": "Cumulative Count",
            "Annotation": "Annotation",
            "Attachment": "Attachment",
            "pNet_correlationCoefficient": "Correlation Coefficient",
            "USL_Choice1": "First Choice Upper Tolerance",
            "USL_Choice2": "Second Choice Upper Tolerance",
            "USL_Choice3": "Third Choice Upper Tolerance",
            "LSL_Choice1":"First Choice Lower Tolerance",
            "LSL_Choice2": "Second Choice Lower Tolerance",
            "LSL_Choice3": "Third Choice Lower Tolerance"

        };
    }
}

function SubgroupContainsStatus(subgroup, status) {
    var statusExists = false;
    if (subgroup.statuses && subgroup.statuses.length > 0) {
        for (var i = 0; i < subgroup.statuses.length; i++)

            if (strToLowerCase(subgroup.statuses[i].category)== status.toLowerCase()) {
                statusExists = true;
                break;
            }
    }
    return statusExists;
}

function parseChartType(chartType) {
    if (chartType && typeof chartType === 'string') {
        for (var key in chartTypes) {
            if (key.toLowerCase() === chartType.toLowerCase())
                return chartTypes[key];
        }
    }
    throw "Invalid Char type in parameter, valid values can be one from these -> xb_s, mxb_ms, xb_R, mxb_mR, x_ms, x_mR, xb, s, r, hist_S, hist_T ";
}

function parseAttrChartType(chartType) {
    if (chartType && typeof chartType === 'string') {
        for (var key in attrChartTypes) {
            if (key.toLowerCase() === chartType.toLowerCase())
                return attrChartTypes[key];
        }
    } else
        throw "Invalid Char type in parameter, valid values can be one from these -> xb_s, mxb_ms, xb_R, mxb_mR, x_ms, x_mR, xb, s, r,hist_S,hist_T ";
}

function getLocalizedText(key, locale) {
    var text = '';
    if (key && locale && Highcharts.uiLocale[locale]) {
        var matchedKey = Object.keys(Highcharts.uiLocale[locale]).filter(function (k) {
            return k.toLowerCase() === key.toLowerCase();
        });
        if (matchedKey.length > 0) {
            text = Highcharts.uiLocale[locale][matchedKey[0]];
        }
    } else {
        console.log('Text not found for key ' + key + ' and locale ' + locale);
    }
    return text;
}

function getVoilations(inputObj) {
    var lowercaseStatuses = [];
    if (inputObj) {
        lowercaseStatuses =
            $.map(inputObj, function (el) {
                if (el.category)
                    return el.category.toString().toLowerCase();
            });
    }
    return lowercaseStatuses;
}

function roundFloat(inputFloat, decimalPlaces, nullReturn) {
    var precision = parseInt(decimalPlaces);
    if (isNaN(precision) || isNullOrUndefined(precision))
        precision = 0;
    if (!isNaN(parseFloat(inputFloat)) && precision != 0) {
        return parseFloat(parseFloat(inputFloat.toString()).toFixed(precision)).toFixed(precision);
    } else if (!isNaN(parseFloat(inputFloat)) && !isNullOrUndefined(precision))
        return parseFloat(inputFloat);

    if (nullReturn === true)
        return null

    return inputFloat;
}

function isNullOrUndefined(input) {
    // if the input is Array we need to make sure that the content of the Array is also null or undefined 
    if (Array.isArray(input)) {
        input.forEach(function (i) {
            return i === null || i === undefined
        });
    }
    return input === null || input === undefined;
}

function getColoredSeriesZone(interval, color) {
    var zone = [];

    for (var i = 0; i < interval.length; i++) {
        zone.push({ value: interval[i][0] }, { value: interval[i][1], color: color });
    }

    return zone;

}

function getSequence(violatedSubgroupsNumber) {
    var tempSeq = [];
    var interval = [];
    //TODO: here should be check added against violatedSubgroupsNumber for null or it's type should be checked against array
    tempSeq.push(violatedSubgroupsNumber[0]);
    for (var i = 1; i < violatedSubgroupsNumber.length; i++) {
        if (Math.abs(violatedSubgroupsNumber[i + 1] - violatedSubgroupsNumber[i]) == 1)
            tempSeq.push(violatedSubgroupsNumber[i])
        else {
            tempSeq.push(violatedSubgroupsNumber[i]);
            interval.push([Math.min.apply(null, tempSeq), Math.max.apply(null, tempSeq)]);
            tempSeq = [];
        }
    }

    return interval;
}

function createCustomHighStockSymbols() {
    if (Highcharts && Highcharts.SVGRenderer) {
        
        Highcharts.SVGRenderer.prototype.symbols.cross = function (x, y, w, h) {
            return ['M', x, y, 'L', x + w, y + h, 'M', x + w, y, 'L', x, y + h, 'z'];
        };
        
        Highcharts.SVGRenderer.prototype.symbols.box = function (x, y, w, h) {
            return ['M', x, y, 'L', x + w, y, 'M', x + w, y, 'L', x + w, y + h, 'M', x + w, y + h, 'L', x, y + h, 'M', x, y + h, 'L', x, y, 'z'];
        };

        (function (H) {
            var addEvent = H.addEvent,
                each = H.each,
                Renderer = H.Renderer,
                SVGRenderer = H.SVGRenderer,
                VMLRenderer = H.VMLRenderer,
                symbols = SVGRenderer.prototype.symbols,
                simpleShapes = ['circle', 'square', 'diamond'],
                additionalShapes = ['flag', 'circlepin', 'squarepin', 'diamondpin'];

            // create the circlepin, squarepin and diamondpin icons with anchor
            each(simpleShapes, function (shape) {
                symbols[shape + 'pin'] = function (x, y, w, h, options) {

                    var anchorX = options && options.anchorX,
                        anchorY = options && options.anchorY,
                        path,
                        labelTopOrBottomY;

                    // For single-letter flags, make sure circular flags are not taller than their width
                    if (shape === 'circle' && h > w) {
                        x -= Math.round((h - w) / 2);
                        w = h;
                    }

                    path = symbols[shape](x, y, w, h);

                    if (anchorX && anchorY) {
                        // if the label is below the anchor, draw the connecting line from the top edge of the label
                        // otherwise start drawing from the bottom edge
                        labelTopOrBottomY = (y > anchorY) ? y : y + h;
                        path.push('M', anchorX, labelTopOrBottomY, 'L', anchorX, anchorY);
                    }

                    return path;
                };
            });

            // The symbol callbacks are generated on the SVGRenderer object in all browsers. Even
            // VML browsers need this in order to generate shapes in export. Now share
            // them with the VMLRenderer.
            if (Renderer === VMLRenderer) {
                each(additionalShapes, function (shape) {
                    VMLRenderer.prototype.symbols[shape] = symbols[shape];
                });
            }
        }(Highcharts));
        
    }
}
function resolveOverlappingFlags() {
    (function (H) {
        function collide(a, b) {
            return !(b.x > a.x + a.width || b.x + b.width < a.x || b.y > a.y + a.height || b.y + b.height < a.y);
        }

        H.wrap(H.seriesTypes.flags.prototype, 'drawPoints', function (p) {
            var series = this,
                chart = series.chart,
                overlap = true,
                counter = 0,
                index,
                offset = series.options.stackDistance,
                currentBBox,
                compareBBox,
                compareSeries;

            p.call(this);

            while (overlap && counter < 100) { // as long as flags do overlap, move them. Extra limiter up to 100 iterations.
                overlap = false;
                H.each(series.points, function (currentPoint) {
                    if (currentPoint.graphic) { // only existing point with label

                        index = 0;
                        currentBBox = {
                            x: currentPoint.graphic.translateX,
                            y: currentPoint.graphic.translateY,
                            width: currentPoint.graphic.width,
                            height: currentPoint.graphic.height
                        };

                        for (; series.index - index >= 0; index++) { // compare only with previous series

                            compareSeries = chart.series[index];

                            if (compareSeries.options.type === "flags") { // only flag type seires

                                H.each(compareSeries.points, function (comparePoint) { // compare current label with all others
                                    if (compareSeries === series && comparePoint.index >= currentPoint.index) {
                                        return; // perf
                                    }

                                    if (comparePoint.graphic) { // only existing point with label

                                        compareBBox = {
                                            x: comparePoint.graphic.translateX,
                                            y: comparePoint.graphic.translateY,
                                            width: comparePoint.graphic.width,
                                            height: comparePoint.graphic.height
                                        };

                                        if (collide(currentBBox, compareBBox)) { // when collide, move current label to top
                                            overlap = true;
                                            currentPoint.graphic.attr({
                                                y: currentPoint.graphic.attr("y") - offset,
                                                anchorY: currentPoint.plotY
                                            });
                                            currentPoint.tooltipPos[1] -= offset;
                                        }
                                    }
                                });
                            }
                        }
                    }
                });
                counter++;
            }
        });
    })(Highcharts);
}
function init() {
    if (!String.prototype.startsWith) {
        String.prototype.startsWith = function (searchString, position) {
            position = position || 0;
            return this.indexOf(searchString, position) === position;
        };
    }

    if (!String.prototype.format) {// "to use {0}".format(1)
        String.prototype.format = function () {
            var args = arguments;
            return this.replace(/{(\d+)}/g, function (match, number) {
                return typeof args[number] != 'undefined'
                    ? args[number]
                    : match
                    ;
            });
        };
    }
    if (Highcharts && !Highcharts.uiLocale)
        Highcharts.uiLocale = {};
    setDefaultLocaleText();
    createCustomHighStockSymbols();
    resolveOverlappingFlags();
}
function strToLowerCase(str) {
    if (typeof str === 'string')
        return str.toLowerCase();
}
//function moving_average(array) {

//    for (var i = 1; i < array.length; i++) {
//        var result = []
//        result.push(array[i] - array[i - 1]);
//    }
//    var res = result.reduce(function (a, b) { return a+b },0) / result.length;

//    return res;
//}
function getStatuse(statuses,key) {
    var status = [];
    if (statuses && statuses.filter) {
        status = statuses.filter(function (s) {
            if (s.category && s.category.toLowerCase() === key) {
                return true;
            }
        });
    }

    return statuses;
}
function isEmpty(str) {
    return (str && str.length === 0);
}

function validateLegendPositioning(legendSettings) {
    if (legendSettings != null) {
        if (typeof legendSettings.enabled === 'undefined') {
            legendSettings.enabled = true;
        }
        if (typeof legendSettings.align === 'undefined') {
            legendSettings.align = 'right';
        }
        if (typeof legendSettings.verticalAlign === 'undefined') {
            if (legendSettings.align === 'right' || legendSettings.align === 'left') {
                legendSettings.verticalAlign = 'top';
            } else {
                legendSettings.verticalAlign = 'bottom';
            }
        }
        if (typeof legendSettings.layout === 'undefined') {
            if (legendSettings.align === 'center') {
                legendSettings.layout = 'horizontal';
            } else {
                legendSettings.layout = 'vertical';
            }
        }
        if (typeof legendSettings.x === 'undefined') {
            legendSettings.x = 0;
        }
        if (typeof legendSettings.y === 'undefined') {
            legendSettings.y = 0;
        }
        if (legendSettings.align === 'right' && legendSettings.verticalAlign === 'top' && legendSettings.layout === 'vertical') {             
            legendSettings.y = 100;
        }
    } else {
        legendSettings.enabled = true;
        legendSettings.align = 'right';
        legendSettings.verticalAlign = 'top';
        legendSettings.layout = 'vertical';
        legendSettings.x = 0;
        legendSettings.y = 100;
    }
}

function seriesVisibility(seriesId, seriesData, hiddenSeries, showingDefault) {
    var hasData = (seriesData || []).filter(function (k) { return !isNullOrUndefined(k); }).length > 0;
    var hiddenSerie = hiddenSeries.filter(function (f) { return !isNullOrUndefined(f) && !isNullOrUndefined(f.seriesId) && !isNullOrUndefined(seriesId) && f.seriesId.toLowerCase() === seriesId.toLowerCase() });
    hiddenSerie = hiddenSerie && hiddenSerie.length > 0 ? hiddenSerie[0] : null;
       
    // var hiddenSerie = findSeries(seriesId, hiddenSeries);
    var showInChart = showingDefault && hasData;// if series has data then display in legend and series
    var showInLegend = showingDefault && hasData;    

    if (hiddenSerie) {
        showInChart = hiddenSerie.showInChart && hasData; // should be AND here with hasData, otherwise a user can enable in legend while series has no data.
        showInLegend = hiddenSerie.showInLegend && hasData;
    }    
    var result = {
        "showInChart": showInChart, "showInLegend": showInLegend }
    return result;
}

//A common function To export chart as base64 string
function createBase64Image(chartApi, options) {
    if (!chartApi || !options) {
        console.log("Either Chart is not rendered or no options are provided.");
        return;
    }

    options = $.extend({ width: chartApi.container.clientWidth, height: chartApi.container.clientHeight }, options);

    var s1 = chartApi.getSVG({
        exporting: {
            sourceWidth: options.width || chartApi.container.clientWidth,
            sourceHeight: options.height || chartApi.container.clientHeight
        }
    });

    var c = document.createElement("canvas");

    if (window.canvgv2) { // FOR IE
        canvgv2(c, s1, {
            ignoreClear: true,
            width: options.width,
            height: options.height,
            fillStyle: '#FFF',
            renderCallback:
                function () {
                    var base64Input = document.getElementById(options.containerId || '');
                    if (base64Input) {
                        base64Input.value = '';
                        base64Input.value = c.toDataURL('image/jpeg');
                    }
                    if (options.success && typeof options.success === 'function')
                        options.success();
                }
        });

        return;
    }

    var svgurl = window.URL.createObjectURL(new window.Blob([s1], {
        type: 'image/svg+xml;charset-utf-16'
    }));

    var img = new Image();
    img.setAttribute('crossOrigin', 'anonymous');

    img.onload = function () {
        console.log('image drawn');
        createBase64String();
    }

    img.onerror = function (e) {
        console.log(e, "unable to load the svg image");
        if (options.error && typeof options.error === 'function')
            options.error();
    }
    img.src = svgurl;


    function createBase64String() {
        if (!options.containerId) {
            console.log('No export container ID for <input type=hidden> provided');
            return;
        }

        var canvas = document.createElement('canvas'),
            ctx = canvas.getContext && canvas.getContext('2d'),
            dataURL,
            base64Input = document.getElementById(options.containerId || ''),
            scale = 1;


        if (!ctx) {
            alert("Not a canvas supported browser");

        }
        else {
            canvas.height = img.height * scale;
            canvas.width = img.width * scale;
            ctx.fillStyle = "#FFF";
            ctx.fillRect(0, 0, canvas.width, canvas.height);
            ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

            // Now we try to get the contents of the canvas.
            try {
                if (base64Input) {
                    dataURL = canvas.toDataURL('image/jpeg');
                    base64Input.value = '';
                    base64Input.value = dataURL;

                    if (options.success && typeof options.success === 'function')
                        options.success();
                }
            } catch (e) {
                console.log('Unable to create base64 Image. Try to add reference of canvg.js');
                if (options.error && typeof options.error === 'function')
                    options.error(e);
            }
        }
    }
}

init();

}) (jQuery);