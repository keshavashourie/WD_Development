// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
/// <reference path="~/Scripts/jquery/jquery.min.js" />
/// <reference path="~/Scripts/jquery/jquery-ui.min.js" />
Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");
Type.registerNamespace("Camstar.WebPortal.Personalization");

Camstar.WebPortal.FormsFramework.WebControls.Scales = function(element) 
{
    Camstar.WebPortal.FormsFramework.WebControls.Scales.initializeBase(this, [element]);

    this._cssClass = null;
    this._targetMin = null;
    this._targetMax = null;
    this._targetValue = null;
    this._scaleMinValue = null;
    this._scaleMaxValue = null;
    this._uom = null;
    this._hiddenDataControl = null;
    this._zoom = null;
    this._renderScaleSettings = null;
    this._zoomPercentage = null;
    this._aboveMaxZoneColor = null; // #FFFFFF
    this._targetZoneColor = null;
    this._belowMinZoneColor = null;
    this._defaultValue = null;
    this._decimalSeparator = null;
    this.greenRGB = null; // 255,255,255
    this.yellowRGB = null;
    this.redRGB = null;
    this.backgroundOpacity = null;
    this.decimaScale = null;
    this.roundingRule = null;

};

Camstar.WebPortal.FormsFramework.WebControls.Scales.prototype =
{
    initialize: function()
    {
        Camstar.WebPortal.FormsFramework.WebControls.Scales.callBaseMethod(this, "initialize");
        this._zoom = false;
        this.adjustControlCollors();
        this.backgroundOpacity = "0.5";

        if (this._hiddenDataControl && this._hiddenDataControl.value) {
            var value = this.splitValueAndUom(this._hiddenDataControl.value);
            if (value[1].length)
                this._uom = value[1];
            this.drawControl(value[0]);
        }
        else
            this.drawControl(0);
    },

    dispose: function()
    {
        this._cssClass = null;
        this._targetMin = null;
        this._targetMax = null;
        this._targetValue = null;
        this._scaleMinValue = null;
        this._scaleMaxValue = null;
        this._uom = null;
        this._hiddenDataControl = null;
        this._zoom = null;
        this._renderScaleSettings = null;
        this._zoomPercentage = null;
        this._aboveMaxZoneColor = null;
        this._targetZoneColor = null;
        this._belowMinZoneColor = null;
        this.greenRGB = null;
        this.yellowRGB = null;
        this.redRGB = null;
        this.backgroundOpacity = null;
        this._decimalSeparator = null;
        this.decimaScale = null;
        this.roundingRule = null;
        
        Camstar.WebPortal.FormsFramework.WebControls.Scales.callBaseMethod(this, "dispose");
    },

    adjustControlCollors: function() {
        this.greenRGB = this.convertHexToRGB(this._targetZoneColor);
        this.yellowRGB = this.convertHexToRGB(this._belowMinZoneColor);
        this.redRGB = this.convertHexToRGB(this._aboveMaxZoneColor);
    },

    convertHexToRGB: function(hex) {
        var hexRegex = /^#?([\da-fA-F]{2})([\da-fA-F]{2})([\da-fA-F]{2})$/; // #FFFFFF.
        var rgb = null;
        var matches;
        if (hex) {
            matches = hexRegex.exec(hex);
            if (matches && matches.length === 4) {
                rgb = parseInt(matches[1], 16) + "," + parseInt(matches[2], 16) + "," + parseInt(matches[3], 16);
            }
        }
        return rgb;
    },

    drawControl: function (actualValue)
    {
        var $control = $(this._element);
        if (!$control.hasClass("scales"))
            $control.addClass("scales");
        if (this._cssClass && !$control.hasClass(this._cssClass))
            $control.addClass(this._cssClass);

        this.zoomScales(actualValue);
    },

    // renders Scales parts without actual value.
    drawScalesArea: function ()
    {
        var $mainScaleParent = $(".mainScaleParent", $(this._element));
        if ($mainScaleParent.length)
        {
            // remove all controls from the scales.
            $mainScaleParent.empty();
            var $lineDiv = $("<div class='mainScaleLine'></div>");

            // yellow part.
            var $yellowPartDiv = $("<div class='yellow part'></div>");
            $lineDiv.append($yellowPartDiv);
            // green part.
            var $greenPartDiv = $("<div class='green part'></div>");
            $lineDiv.append($greenPartDiv);
            // red part.
            var $redPartDiv = $("<div class='red part'></div>");
            $lineDiv.append($redPartDiv);
            var targetValues = this.getTargetValues();
            if (targetValues === undefined) return false;
            var partWidth = this.calculateScalePartWidth(targetValues);
            if (partWidth)
            {
                var areaWidth = partWidth[0]; // yellow.
                if (areaWidth)
                {
                    $yellowPartDiv.width(areaWidth);
                    if (areaWidth !== "0%")
                    {
                        // yellow border.
                        var $yellowPartBorderDiv = $("<div class='darkYellow part-border'>&nbsp;</div>");
                        $yellowPartBorderDiv.css("left", areaWidth);
                        if (targetValues[1] !== targetValues[0])
                            $lineDiv.append($yellowPartBorderDiv);
                    }
                }
                areaWidth = partWidth[1]; // green.
                if (areaWidth)
                {
                    $greenPartDiv.width(areaWidth);
                    this.drawTargetValue($greenPartDiv);
                }
                areaWidth = partWidth[2]; // red.
                if (areaWidth)
                {
                    $redPartDiv.width(areaWidth);
                    if (areaWidth !== "0%")
                    {
                        // red border.
                        var $redPartBorderDiv = $("<div class='darkRed part-border'>&nbsp;</div>");
                        $redPartBorderDiv.css("right", areaWidth);
                        if (targetValues[0] !== targetValues[2])
                            $lineDiv.append($redPartBorderDiv);
                    }
                }
            }
            $mainScaleParent.append($lineDiv);
        }
        return true;
    },

    // displays actual value on the scale.
    drawActualValue: function (actualValue)
    {
        var value = 0;
        if (actualValue)
            value = parseFloat(actualValue.toString().replace(",", "."));
        if (isNaN(value))
            value = 0;

        var targetValues = this.getTargetValues();
        var targetMin = targetValues[1];
        var targetMax = targetValues[2];

        var calculatedZoomState = false;
        if (this.canMicroView()) {
            var zoomPers = 100 - this._zoomPercentage;
            if (targetMin < 0)
                zoomPers = 100 + this._zoomPercentage;

            if (value >= (zoomPers / 100) * targetMin) // _zoomPercentage % less than target min.
                calculatedZoomState = true;
        }

        // need to zoom-in or zoom-out scales.
        if (this._zoom !== calculatedZoomState)
        {
            this._zoom = calculatedZoomState;
            this.zoomScales(value);
        }
        else
        {
            var left = 0;
            var $cursorHolder = null;
            var cursorData = null;
            $(this._element).removeClass("micro macro");
            if (!this._zoom) // renders macro view.
            {
                cursorData = this.drawMacroView(value);
            }
            else // renders micro view.
            {
                cursorData = this.drawMicroView(value);
            }

            $cursorHolder = cursorData[0];
            left = cursorData[1];

            // move cursor.
            if ($cursorHolder)
            {
                var $cursor = $(".currentValue-parent", $cursorHolder);
                if ($cursor.length)
                    $cursor.css("left", left + "%");
                else
                {
                    var $mainScaleLine = $(".mainScaleLine", $(this._element));
                    $cursor = $(".currentValue-parent", $mainScaleLine);
                    if ($cursor.length)
                    {
                        // remove cursor from previous div area.
                        $cursor.remove();
                    }
                    // create new cursor.
                    else
                    {
                        // draw current value cursor.
                        $cursor = $("<div class='currentValue-parent'></div>");
                        var $currentValueLine = $("<div class='currentValue-line'>&nbsp;</div>");
                        var $currentValueTriangleDown = $("<div class='currentValue-triangle-down'>&nbsp;</div>");
                        var $currentValueTriangleUp = $("<div class='currentValue-triangle-up'>&nbsp;</div>");

                        $cursor.append($currentValueLine);
                        $cursor.append($currentValueTriangleUp);
                        $cursor.append($currentValueTriangleDown);
                    }

                    $cursor.css("left", left + "%");
                    $cursorHolder.prepend($cursor);
                }
            }
        }

        var $valueDiv = $(".actualValueValue", $(this._element));
        if ($valueDiv.length) 
        {
            var actualVal = value;
            $valueDiv.removeClass("darkYellow darkGreen darkRed");
            $valueDiv.css("color", "");
            if (!value)
                actualVal = "0.0";
            else
            {
                if (value >= targetMin && value <= targetMax)
                {
                    if (!$valueDiv.hasClass("darkGreen"))
                        $valueDiv.addClass("darkGreen");
                }
                else if (value > targetMax)
                {
                    if (!$valueDiv.hasClass("darkRed"))
                        $valueDiv.addClass("darkRed");
                }
            }
            if (this._uom)
                actualVal += " " + this._uom;
            $valueDiv.html(actualVal.toString().replace(".", this._decimalSeparator));
        }
        var $mainScaleParent = $(".mainScaleParent", $(this._element));
        if ($mainScaleParent.length) {
            if (this.greenRGB) {
                $(".green", $mainScaleParent).css("background-color", "rgba(" + this.greenRGB + "," + this.backgroundOpacity + ")");
                $(".darkGreen", $mainScaleParent).css("background-color", "rgb(" + this.greenRGB + ")");
                if ($valueDiv.hasClass("darkGreen"))
                    $valueDiv.css("color", "rgb(" + this.greenRGB + ")");
            }
            if (this.yellowRGB) {
                $(".yellow", $mainScaleParent).css("background-color", "rgba(" + this.yellowRGB + "," + this.backgroundOpacity + ")");
                $(".darkYellow", $mainScaleParent).css("background-color", "rgb(" + this.yellowRGB + ")");
            }
            if (this.redRGB) {
                $(".red", $mainScaleParent).css("background-color", "rgba(" + this.redRGB + "," + this.backgroundOpacity + ")");
                $(".darkRed", $mainScaleParent).css("background-color", "rgb(" + this.redRGB + ")");
                if ($valueDiv.hasClass("darkRed"))
                    $valueDiv.css("color", "rgb(" + this.redRGB + ")");
            }
        }
    },

    calculateScalePartWidth: function (targetValues)
    {
        var retWidth = null; // yellow, green and red area width in pixels.
        var $mainScaleParent = $(".mainScaleParent", $(this._element));
        if ($mainScaleParent.length)
        {
            if (!this._zoom)
            {
                var targetMin = targetValues[1];
                var targetMax = targetValues[2];
                var scaleMin = targetValues[3];
                var scaleMax = targetValues[4];
                var yellowWidth = 0;
                if (targetMin !== scaleMin) // yellow is empty when targetMin = scaleMin.
                    yellowWidth = Math.floor(100 * (targetMin - scaleMin) / (scaleMax - scaleMin));
                var greenwWidth = Math.floor(100 * (targetMax - targetMin) / (scaleMax - scaleMin));
                var redWidth = 100 - yellowWidth - greenwWidth;
                if (targetMax === scaleMax) // red is empty when targetMax = scaleMax.
                {
                    redWidth = 0;
                    greenwWidth = 100 - yellowWidth;
                }
                retWidth = [yellowWidth + "%", greenwWidth + "%", redWidth + "%"];
            }
            else
                retWidth = ["10%", "80%", "10%"];
        }
        return retWidth;
    },
    roundDownToDecimalPlaces: function (number, decimalPlaces) {
        let multiplier = Math.pow(10, decimalPlaces);
        return Math.floor(number * multiplier) / multiplier;
    },
    roundUpToDecimalPlaces: function (number, decimalPlaces) {
        let multiplier = Math.pow(10, decimalPlaces);
        return Math.ceil(number * multiplier) / multiplier;
    },
        roundDefaultToDecimalPlaces: function (number, decimalPlaces) {
        let multiplier = Math.pow(10, decimalPlaces);
            return Math.round(number * multiplier) / multiplier;
    }
    ,
    AddScalePart: function (input) {
        let digitsAfterPoint = 0;

        if (Number.isInteger(input)) {
            digitsAfterPoint = 0;
        }
        else {
            digitsAfterPoint = input.toString().split('.')[1].length;
        }
        let numberAsString = input.toString();

        if (digitsAfterPoint < this.decimaScale) {
            for (let i = 0; i < this.decimaScale - digitsAfterPoint; i++) {
                if (digitsAfterPoint === 0 && i === 0) {
                    numberAsString = `${numberAsString}.0`;
                } else {
                    numberAsString = `${numberAsString}0`;
                }
            }
        } else {
            if (this.roundingRule === 1) {
                return this.roundDownToDecimalPlaces(input, this.decimaScale)
            }
            else
            if (this.roundingRule === 2) {
                return this.roundUpToDecimalPlaces(input, this.decimaScale)
                }
            else
                return this.roundDefaultToDecimalPlaces(input, this.decimaScale)
        }

        return numberAsString;


    },
   
    // renders macroview.
    // returns cursor holder and calculated left position.
    drawMacroView: function (value)
    {
        var targetValues = this.getTargetValues();
        var targetMin = targetValues[1];
        var targetMax = targetValues[2];
        var scaleMin = targetValues[3];
        var scaleMax = targetValues[4];

        var cursorData;
        var left = 0;
        var $cursorHolder;
        var $yellowPart = $(".part.yellow", $(this._element));
        var $greenPart = $(".part.green", $(this._element));
        var $redPart = $(".part.red", $(this._element));

        // display target-min and target-max values.
        if (this._renderScaleSettings)
        {
            this.drawTargetMinMax(targetMin, targetMax);
            this.drawScaleMinMax();
        }
        if (value < scaleMin)
            value = scaleMin;

        if (value === scaleMin && (targetMin > scaleMin)) // targetMin > scaleMin - no yellowPart.
        {
            // clean up.
            $(".darkPart.darkGreen", $greenPart).hide();
            $yellowPart.removeClass("darkYellow");

            $cursorHolder = $yellowPart;
            $yellowPart.empty(); // remove dark yellow part.
        }
        else if (value <= targetMin && (targetMin > scaleMin)) // cursor within yellow area.
        {
            cursorData = this.cursorYellowArea($yellowPart, $greenPart, $redPart, targetValues, value);
            $cursorHolder = cursorData[0];
            left = cursorData[1];
        }
        else if ((value > targetMax) && (targetMax !== scaleMax)) // cursor within red area.
        {
            cursorData = this.cursorRedArea($greenPart, $redPart, targetValues, value);
            $cursorHolder = cursorData[0];
            left = cursorData[1];
        }
        else // cursor within green area.
        {
            cursorData = this.cursorGreenArea($greenPart, $redPart, targetValues, value);
            $cursorHolder = cursorData[0];
            left = cursorData[1];
        }
        if (!$(this._element).hasClass("macro"))
            $(this._element).addClass("macro");

        return [$cursorHolder, left];
    },

    // renders microview. (10%, 80%, 10%).
    // returns cursor holder and calculated left position.
    drawMicroView: function (value)
    {
        var targetValues = this.getTargetValues();
        var targetMin = targetValues[1];
        var targetMax = targetValues[2];

        var cursorData;
        var left;
        var $cursorHolder;
        var $yellowPart = $(".part.yellow", $(this._element));
        var $greenPart = $(".part.green", $(this._element));
        var $redPart = $(".part.red", $(this._element));

        $yellowPart.addClass("darkYellow");

        // display target-min and target-max values.
        this.drawTargetMinMax(targetMin, targetMax);

        if (value < targetMin) // cursor within yellow area.
        {
            cursorData = this.cursorYellowArea($yellowPart, $greenPart, $redPart, targetValues, value);
            $cursorHolder = cursorData[0];
            left = cursorData[1];
        }
        else if (value <= targetMax) // cursor within green area.
        {
            cursorData = this.cursorGreenArea($greenPart, $redPart, targetValues, value);
            $cursorHolder = cursorData[0];
            left = cursorData[1];
        }
        else // cursor within red area.
        {
            cursorData = this.cursorRedArea($greenPart, $redPart, targetValues, value);
            $cursorHolder = cursorData[0];
            left = cursorData[1];
        }
        if (!$(this._element).hasClass("micro"))
            $(this._element).addClass("micro");

        return [$cursorHolder, left];
    },

    // draws cursor within red area.
    cursorRedArea: function ($greenPart, $redPart, targetValues, value)
    {
        var cursorData = [];
        var targetMax = targetValues[2];
        var scaleMax = targetValues[4];

        // clean up.
        $(".darkPart.darkGreen", $greenPart).hide();
        $greenPart.addClass("darkGreen");
        cursorData[0] = $redPart;

        // highlight value level.
        var $darkRedPart = $(".darkPart.darkRed", $redPart);
        if (!$darkRedPart.length)
        {
            $darkRedPart = $("<div class='darkPart darkRed'>&nbsp;</div>");
            $redPart.append($darkRedPart);
        }
        else
            $darkRedPart.show();

        var left;
        var max = scaleMax;
        if (value < max)
            left = Math.floor(100 * (value - targetMax) / (max - targetMax));
        else
            left = 100;

        cursorData[1] = left;
        $darkRedPart.width(left + "%");

        return cursorData;
    },

    // draws cursor within green area.
    cursorGreenArea: function ($greenPart, $redPart, targetValues, value)
    {
        var cursorData = [];
        var targetMin = targetValues[1];
        var targetMax = targetValues[2];

        // clean up.
        $(".darkPart.darkRed", $redPart).hide();
        $greenPart.removeClass("darkGreen");

        cursorData[0] = $greenPart;
        // highlight value level.
        var $darkGreenPart = $(".darkPart.darkGreen", $greenPart);
        if (!$darkGreenPart.length) {
            $darkGreenPart = $("<div class='darkPart darkGreen'>&nbsp;</div>");
            $greenPart.append($darkGreenPart);
        }
        else
            $darkGreenPart.show();

        var left = 0;
        if (targetMax !== targetMin)
            left = Math.floor(100 * (value - targetMin) / (targetMax - targetMin));
        $darkGreenPart.width(left + "%");
        cursorData[1] = left;

        return cursorData;
    },

    // draws cursor within yellow area.
    cursorYellowArea: function ($yellowPart, $greenPart, $redPart, targetValues, value)
    {
        var cursorData = [];
        var targetMin = targetValues[1];
        var scaleMin = targetValues[3];

        // clean up.
        $(".darkPart.darkGreen", $greenPart).hide();
        $(".darkPart.darkRed", $redPart).hide();
        $yellowPart.removeClass("darkYellow");

        cursorData[0] = $yellowPart;
        // highlight value level.
        var $darkYellowPart = $(".darkPart.darkYellow", $yellowPart);
        if (!$darkYellowPart.length)
        {
            $darkYellowPart = $("<div class='darkPart darkYellow'>&nbsp;</div>");
            $darkYellowPart.css("float", "left");
            $yellowPart.append($darkYellowPart);
        }
        else
            $darkYellowPart.show();

        var left = Math.floor(100 * (value - scaleMin) / (targetMin - scaleMin));
        $darkYellowPart.width(left + "%");
        cursorData[1] = left;

        return cursorData;
    },

    // Draws target value poiner + text value.
    drawTargetValue: function ($greenPartDiv)
    {
        var targetValues = this.getTargetValues();
        var targetValue = targetValues[0];
        var targetMin = targetValues[1];
        var targetMax = targetValues[2];

        var leftPos;
        if (targetValue <= targetMin)
            leftPos = 0;
        else if (targetValue >= targetMax)
            leftPos = 100;
        else
            leftPos = Math.floor(100 * (targetValue - targetMin) / (targetMax - targetMin));

        // black line.
        var $targetValueLine = $("<div class='targetValue-line'>&nbsp;</div>");
        $targetValueLine.css("left", leftPos + "%");
        $greenPartDiv.append($targetValueLine);

        // black triangle.
        var $targetValueTriangle = $("<div class='targetValue-triangle'>&nbsp;</div>");
        $targetValueTriangle.css("left", leftPos + "%");
        $greenPartDiv.append($targetValueTriangle);
        // black value.
        var $targetValueValue = $("<div class='targetValue-value'></div>");
        //Add scale part
        if (this.decimaScale != null && typeof this.decimaScale !== 'undefined') {
            targetValue = this.AddScalePart(targetValue);
        }

        $targetValueValue.html(targetValue.toString().replace(".", this._decimalSeparator));
        $targetValueValue.css("left", leftPos + "%");
        $targetValueValue.css("margin-left", -3 * targetValue.toString().length + "px");
        $greenPartDiv.append($targetValueValue);
    },

    drawTargetMinMax: function (targetMin, targetMax)
    {
        // display target-min and target-max values.
        var targetValues = this.getTargetValues();
        var partWidth = this.calculateScalePartWidth(targetValues);
        var $mainScaleParent = $(".mainScaleParent", $(this._element));
        var $targetMinValue = $(".targetMinValue-value", $(this._element));
        if (partWidth)
        {
            if (this._zoom || (targetValues[1] !== targetValues[3])) // if targetMin != scaleMin.
            {
                if (!$targetMinValue.length)
                {
                    $targetMinValue = $("<div style='left: " + partWidth[0] + ";' class='targetMinValue-value'></div>");

                    if (this.decimaScale != null && typeof this.decimaScale !== 'undefined') {
                        targetMin = this.AddScalePart(targetMin);
                    }
                    else {
                        this.doubleToString(targetMin, 4);
                    }
                                   
                    if (partWidth[0] !== "0%") // there is space on left.
                        $targetMinValue.css("margin-left", -3 * targetMin.length + "px");
                    $targetMinValue.html(targetMin);
                    $mainScaleParent.append($targetMinValue);
                }
            }
            else
                $targetMinValue.remove();

            var $targetMaxValue = $(".targetMaxValue-value", $(this._element));
            if (this._zoom || (targetValues[2] !== targetValues[4])) // if targetMax != scaleMax.
            {
                if (!$targetMaxValue.length) {
                    $targetMaxValue = $("<div style='right: " + partWidth[2] + ";' class='targetMaxValue-value'></div>");
                    if (this.decimaScale != null && typeof this.decimaScale !== 'undefined') {
                        targetMax = this.AddScalePart(targetMax);
                    }
                    else {
                        this.doubleToString(targetMax, 4);
                    }

                    $targetMaxValue.css("margin-right", -2 * targetMax.length + "px");
                    $targetMaxValue.html(targetMax);
                    $mainScaleParent.append($targetMaxValue);
                }
            }
            else{

                $targetMaxValue.remove();
            }            

            if (targetValues[0] === targetValues[1])
                $targetMinValue.remove();
            if (targetValues[0] === targetValues[2])
                $targetMaxValue.remove();
        }
    },

    drawScaleMinMax: function ()
    {
        // display scale-min and scale-max values.
        var targetValues = this.getTargetValues();
        var $mainScaleParent = $(".mainScaleParent", $(this._element));

        var $scaleMinValue = $(".scaleMinValue-value", $(this._element));
        if (!$scaleMinValue.length)
        {
            $scaleMinValue = $("<div class='scaleMinValue-value'></div>");
            $scaleMinValue.html(this.doubleToString(targetValues[3], 4));
            $mainScaleParent.append($scaleMinValue);
        }

        var $scaleMaxValue = $(".scaleMaxValue-value", $(this._element));
        if (!$scaleMaxValue.length)
        {
            $scaleMaxValue = $("<div class='scaleMaxValue-value'></div>");
            $scaleMaxValue.html(this.doubleToString(targetValues[4], 4));
            $mainScaleParent.append($scaleMaxValue);
        }
    },

    doubleToString: function (number, precision)
    {
        var multi = Math.pow(10, precision);
        return (Math.round(number * multi) / multi).toString().replace(".", this._decimalSeparator);
    },

    // Draw MacroView when both TargetMin and TargetMax specified.
    canMicroView: function ()
    {
        return this._targetMax !== null && this._targetMin !== null && this._targetMax !== this._targetMin;
    },

    zoomScales: function(actualValue)
    {
        var res = this.drawScalesArea();
        if(!res) return;
        this.drawActualValue(actualValue);
    },

    // 0 - targetValue, 1 - targetMin, 2 - targetMax, 3 - scaleMin, 4 - scaleMax
    getTargetValues: function ()
    {
        var targetValues = [];
        var scaleMinSpecified = true;
        var scaleMinValue = this._scaleMinValue;
        if (!this._scaleMinValue) {
            scaleMinValue = "0";
            scaleMinSpecified = false;
        }
            
        // if targetMin empty - use scaleMin.
        var targetMinStr = this._targetMin;
        if (!targetMinStr)
            targetMinStr = scaleMinValue;

        // if targetMax empty - use scaleMax.
        var targetMaxStr = this._targetMax;
        if (!targetMaxStr)
            targetMaxStr = this._scaleMaxValue;

        var targetValue = parseFloat(this._targetValue.replace(",", "."));
        var targetMin = parseFloat(targetMinStr.replace(",", "."));
        var targetMax = parseFloat(targetMaxStr.replace(",", "."));

        targetValues[0] = targetValue;
        targetValues[1] = targetMin;
        targetValues[2] = targetMax;

        var scaleMin = parseFloat(scaleMinValue.replace(",", "."));
        if (targetMin < 0 && !scaleMinSpecified)
            scaleMin = targetMin;

        var scaleMax;
        if (this._scaleMaxValue)
            scaleMax = parseFloat(this._scaleMaxValue.replace(",", "."));
        else
            scaleMax = (targetMax - scaleMin) * 0.1 + targetMax;

        targetValues[3] = scaleMin;
        targetValues[4] = scaleMax;

        if (!targetMaxStr || !this.validate(scaleMin, targetMin, targetValue, targetMax, scaleMax))
        {
            // setTimeout waits when __page variable will be initialized.
            setTimeout(function ()
            {
                __page.getLabel('Lbl_ScaleNotConfigured', function (response)
                {
                    if ($.isArray(response))
                        __page.displayStatus(response[0].Value, "Warning"); // Scale control is not properly configured label.
                    else
                        __page.displayStatus("The scale control cannot be rendered. Please confirm its properties.", "Warning");
                });
            }, 100);

            // don't render control when not properly configured.
            $(this._element).hide();
            return;
        }
        else
            $(this._element).show();
        return targetValues;
    },

    validate: function(scaleMin, targetMin, targetVal, targetMax, scaleMax) {
        return (scaleMin <= targetMin && targetMin <= targetVal && targetVal <= targetMax && targetMax <= scaleMax);
    },

    splitValueAndUom: function(value) {
        var retVal = [];
        retVal.push(value);
        retVal.push(""); // uom part
        if (value) {
            var splitValueUom = value.toString().trim().split(" ");
            if (splitValueUom.length === 2)
                retVal = splitValueUom;
        }
        return retVal;
    },

    directUpdate: function(value)
    {
        Camstar.WebPortal.FormsFramework.WebControls.Scales.callBaseMethod(this, "directUpdate");
        if (value.PropertyKey == eval(Camstar.Ajax.DirectUpdateParameterKeys.Data))
        {
            this.setValue(value.PropertyValue);
        }
    },

    getValue: function()
    {
        if (this._hiddenDataControl)
            return this.splitValueAndUom(this._hiddenDataControl.value)[0];
        else
            return null;
    },

    setValue: function(value)
    {
        if (this._hiddenDataControl)
            this._hiddenDataControl.value = value;

        value = this.splitValueAndUom(value);
        if (value[1].length)
            this._uom = value[1];
        this.drawActualValue(value[0]);
    },

    clearValue: function() 
    {
        this.setValue("");
    },

    get_Data: function() { return this.getValue(); },
    set_Data: function(value) { this.setValue(value); },

    get_Hidden: function() { return this._element.style.display == "none"; },
    set_Hidden: function(value)
    {
        if (value == true)
        {
            this._element.style.display = "none";
            this._label.style.display = "none";
        }
        else
        {
            this._element.style.display = "";
            this._label.style.display = "";
        }
    },

    get_IsEmpty: function() { return !this.getValue(); },

    get_cssClass: function() { return this._cssClass; },
    set_cssClass: function (value) { this._cssClass = value; },

    get_targetMin: function () { return this._targetMin; },
    set_targetMin: function (value) { this._targetMin = value; },
 
    get_targetMax: function () { return this._targetMax; },
    set_targetMax: function (value) { this._targetMax = value; },

    get_targetValue: function () { return this._targetValue; },
    set_targetValue: function (value) { this._targetValue = value; },

    get_decimalSeparator: function () { return this._decimalSeparator; },
    set_decimalSeparator: function (value) { this._decimalSeparator = value; },

    get_scaleMinValue: function () { return this._scaleMinValue; },
    set_scaleMinValue: function (value) { this._scaleMinValue = value; },

    get_scaleMaxValue: function () { return this._scaleMaxValue; },
    set_scaleMaxValue: function (value) { this._scaleMaxValue = value; },

    get_aboveMaxZoneColor: function () { return this._aboveMaxZoneColor; },
    set_aboveMaxZoneColor: function (value) { this._aboveMaxZoneColor = value; },

    get_targetZoneColor: function () { return this._targetZoneColor; },
    set_targetZoneColor: function (value) { this._targetZoneColor = value; },

    get_belowMinZoneColor: function () { return this._belowMinZoneColor; },
    set_belowMinZoneColor: function (value) { this._belowMinZoneColor = value; },

    get_uom: function () { return this._uom; },
    set_uom: function (value) { this._uom = value; },

    get_hiddenData: function () { return this._hiddenDataControl; },
    set_hiddenData: function (value) { this._hiddenDataControl = value; },

    get_renderScaleSettings: function () { return this._renderScaleSettings; },
    set_renderScaleSettings: function (value) { this._renderScaleSettings = value; },

    get_decimalScale: function () { return this.decimaScale; },
    set_decimalScale: function (value) { this.decimaScale = value; },

    get_roundingRule: function () { return this.roundingRule; },
    set_roundingRule: function (value) { this.roundingRule = value; },
    
    get_zoomPercentage: function () { return this._zoomPercentage; },
    set_zoomPercentage: function(value)
    {
        this._zoomPercentage = 10;
        if ($.isNumeric(value))
        {
            if (value >= 0 && value <= 100)
                this._zoomPercentage = value;
        }
    },
    get_defaultValue: function () { return this._defaultValue; },
    set_defaultValue: function (value) { this._defaultValue = value; }

};

Camstar.WebPortal.FormsFramework.WebControls.Scales.registerClass("Camstar.WebPortal.FormsFramework.WebControls.Scales", Camstar.UI.Control);

if (typeof (Sys) !== "undefined") Sys.Application.notifyScriptLoaded();


