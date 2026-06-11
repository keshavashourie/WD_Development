// Copyright Siemens 2023  

/// <reference name="MicrosoftAjax.js"/>
/// <reference path="~/Scripts/ClientFramework/Camstar.UI/Control.js" />
/// <reference path="~/Scripts/jquery/jquery.min.js" />
/// <reference path="~/Scripts/jquery/jquery-ui.min.js" />
Type.registerNamespace("Camstar.WebPortal.FormsFramework.WebControls");

Camstar.WebPortal.FormsFramework.WebControls.PIMetric = function (element) {
    Camstar.WebPortal.FormsFramework.WebControls.PIMetric.initializeBase(this, [element]);

    this._divTooltip = null;
}

Camstar.WebPortal.FormsFramework.WebControls.PIMetric.prototype = {
    
    initialize: function () {

        Camstar.WebPortal.FormsFramework.WebControls.PIMetric.callBaseMethod(this, 'initialize');

        var kpi = this.get_element();
        var divTooltip = this.get_divTooltip();
        if (kpi) {
            $(kpi).hover(function() {
                $('#' + divTooltip).show();
            }, function () {
                $('#' + divTooltip).hide();
            });
        }
    },

    dispose: function () {

        Camstar.WebPortal.FormsFramework.WebControls.PIMetric.callBaseMethod(this, 'dispose');
    },

    get_divTooltip: function () { return this._divTooltip; },
    set_divTooltip: function (value) { this._divTooltip = value; },
}

Camstar.WebPortal.FormsFramework.WebControls.PIMetric.registerClass('Camstar.WebPortal.FormsFramework.WebControls.PIMetric', Camstar.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
