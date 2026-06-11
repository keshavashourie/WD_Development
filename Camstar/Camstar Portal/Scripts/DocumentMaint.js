var documentMaint = (function () {
    'use strict';

    var documentMaintInterface = {
        openModelViewerPopup: openModelViewerPopup
    };

    function openModelViewerPopup(docName, docRev, popupTitle) {

        var url = location.href.substr(0, location.href.lastIndexOf("/")) +
            "/ModelViewerPopup.aspx?IsFloatingFrame=2&name=" + encodeURIComponent(docName) +
            "&revision=" + encodeURIComponent(docRev);

        var isResponsive = document.body.classList.contains("Horizon-theme") || (__page && __page.get_isResponsive());

        pop.showAjax(url, popupTitle,
            isResponsive ? screen.availHeight - 20 : 900/*height*/,
            isResponsive ? screen.availWidth - 80 : 1300 /*width*/,
            0/*top*/, 0/*left*/, true /*showButtons*/,
            "" /*okButtonText*/, ""/*closeButtonText*/,
            this /*element*/, true /*closeOnCancel*/,
            ''/*optionArgs*/, null /*cancelConfirmMsg*/, false /*closeButtonOnly*/, false /*display reset*/);

    }

    return documentMaintInterface;
})();