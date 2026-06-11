var isImageMaint = (function () {
    'use strict';

    var isImageMaintInterface = {
        initializeModeling: initializeModeling,
        initializeDocAttach: initializeDocAttach,
        openModelViewerPopup: openModelViewerPopup
    };

    var _error = "";
    var validExtensions = [];

    var SELECTOR = Object.freeze({
        MODELING_INPUT_FIELD: '#ctl00_WebPartManager_GeneralGroupWP_LocalFileInput_InputField',
        MODELING_UPLOAD_TEXT: '#fileuploadtextctl00_WebPartManager_GeneralGroupWP_LocalFileInput',
        DOCATTACH_INPUT_FIELD: '#ctl00_WebPartManager_AttachDocument_WP_DocumentPath_InputField',
        DOCATTACH_UPLOAD_TEXT: '#fileuploadtextctl00_WebPartManager_AttachDocument_WP_DocumentPath'        
    });

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

    // extensions is a comma separated list of allowed file extensions for images
    // error is string to show during validation if selected file extension is not allowed
    function initializeModeling(extensions, error) {
        validExtensions = extensions.toLowerCase().split(',');
        _error = error;
        setFileSelectValidationHandler(SELECTOR.MODELING_INPUT_FIELD, SELECTOR.MODELING_UPLOAD_TEXT);
    }

    function initializeDocAttach(extensions, error) {
        validExtensions = extensions.toLowerCase().split(',');
        _error = error;
        setFileSelectValidationHandler(SELECTOR.DOCATTACH_INPUT_FIELD, SELECTOR.DOCATTACH_UPLOAD_TEXT);
    }

    function setFileSelectValidationHandler(inputSelector, uploadSelector) {
        var inputFile;

        $(inputSelector).on('change paste keyup', handleInputChange);

        function handleInputChange() {
            inputFile = $(inputSelector).val();
            if (inputFile) {
                setTimeout(handleUploadChange, 1000);
            }
        }

        function handleUploadChange() {
            var uploadFile = $(uploadSelector).val();
            if (!uploadFile)
                uploadFile = inputFile;

            var extension = uploadFile.substr((uploadFile.lastIndexOf('.') + 1));
            if (!isExtensionValid(extension)) {
                $(inputSelector).val("");
                $(uploadSelector).val("");
                $(uploadSelector).attr('value', '');
                alert(_error);
            }
        }
    }

    function isExtensionValid(ext) {
        ext = ext.toLowerCase();
        return validExtensions.some(function (validExt) {
            return ext === validExt;
        });
    }

    return isImageMaintInterface;

})();