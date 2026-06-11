// © 2022 Siemens Product Lifecycle Management Software Inc.

var repairAdvisor =
    (function () {
        'use strict';

        $(function () {
            // make SN Entry take up all available space
            $('div.floating-frame', window.parent.document)
                .css('width', $(window.parent).width() + 'px')
                .css('height', $('div.form-container', window.parent.document).height())
                .css('border-style', 'none')
                .css('top', '0');


            try {
                pop.GetCallerPage().pop.maximize();
            } catch (err) {
            }
        });

        // Interface object - for server side to trigger functions
        //return {
        //    fixPageLayout: fixPageLayout
        //};
    })();

