// Copyright Siemens 2025
//var _barcodeIconsInitialized = false;
function InitBarcodeIcons() {

    // in pure js
    Dynamsoft.Core.CoreModule.engineResourcePaths.rootDirectory = "Scripts/Barcode/distributables";

    Dynamsoft.License.LicenseManager.initLicense("f0068MgAAAFEmC4ivVIQTLlBsKF5h991LcquP8iiZw6QBBACH0r9afFtMZ+OMhnqrfk+ni2bTYiXCXju962GDWAAA3WrodtQ=");
    Dynamsoft.Core.CoreModule.loadWasm(["dbr"]);
    // select all input type text that are not readonly
    const inputSelector = "input[type=text]:not([readonly])";
    const inputDatePickerSelector = "input[class='hasDatepicker']:not([readonly])";

    const popoverOptions = {
        html: true,
        trigger: 'focus',
        content: function () {
            return "<img id='popoverImage' src='Themes/Horizon/Images/cmdBarcode24.svg' class='barcode-icon-popover'>";
        }
    };

    const popoverDatePickerOptions = {
        html: true,
        trigger: 'focus',
        content: function () {
            return "<img id='popoverImageDate' src='Themes/Horizon/Images/cmdBarcode24.svg' class='barcode-icon-popover-date'>";
        }
    };

    let popoverTriggerList = [].slice.call(document.querySelectorAll(inputSelector));
    let popoverDateTriggerList = [].slice.call(document.querySelectorAll(inputDatePickerSelector));


    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl, popoverOptions);
    });

    popoverDateTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl, popoverDatePickerOptions);
    });

    //  Remove any event handlers before re-adding
    $(inputSelector).off('shown.bs.popover');
    $(inputSelector).off('inserted.bs.popover');
    $(inputDatePickerSelector).off('shown.bs.popover');
    $(inputDatePickerSelector).off('inserted.bs.popover');

    $(inputSelector).on('shown.bs.popover', function () {
        $(".barcode-icon-popover").attr("target", $(this).attr("name"))
    });
    $(inputDatePickerSelector).on('shown.bs.popover', function () {
        $(".barcode-icon-popover-date").attr("target", $(this).attr("name"))
    });
    $(inputDatePickerSelector).on('inserted.bs.popover', function () {
        $(".barcode-icon-popover-date")
            .off("mousedown")
            .on("mousedown", function () {
                startScan(this);
            });
    });
    $(inputSelector).on('inserted.bs.popover', function () {
        $(".barcode-icon-popover")
            .off("mousedown")
            .on("mousedown", function () {
                startScan(this);
            });
    });

} // InitBarcodeIcons

function startScan(target) {
    // get the input target name
    target = $(target).attr("target");
    const newDiv = document.createElement('div');
    newDiv.id = 'barcodeScanner';
    if (document.querySelector("#barcodeScanner") == undefined)
        document.body.appendChild(newDiv);
    let barcodeScannerDiv = document.querySelector("#barcodeScanner");
    (async () => {
        let cvRouter = await Dynamsoft.CVR.CaptureVisionRouter.createInstance();
        let cameraView = await Dynamsoft.DCE.CameraView.createInstance();
        let cameraEnhancer = await Dynamsoft.DCE.CameraEnhancer.createInstance(cameraView);
        barcodeScannerDiv.innerHTML = '<button id="barcodeScannerButton" style="position:absolute;right:0;top:0;z-Index:999"><svg width="16" height="16" viewBox="0 0 1792 1792">			<path d="M1490 1322q0 40-28 68l-136 136q-28 28-68 28t-68-28l-294-294-294 294q-28 28-68 28t-68-28l-136-136q-28-28-28-68t28-68l294-294-294-294q-28-28-28-68t28-68l136-136q28-28 68-28t68 28l294 294 294-294q28-28 68-28t68 28l136 136q28 28 28 68t-28 68l-294 294 294 294q28 28 28 68z" />		</svg></button>';
        barcodeScannerDiv.append(cameraView.getUIElement());
        addStyleClassToCameraDiv(barcodeScannerDiv);
        cvRouter.setInput(cameraEnhancer);
        //onclick=closeBarcodeScanner()
        let closebutton = document.querySelector("#barcodeScannerButton");
        closebutton.onclick = await function () {
            cameraEnhancer.close();
            barcodeScannerDiv.style.display = 'none';
            barcodeScannerDiv.innerhtml = '';
        };
        cvRouter.addResultReceiver({
            onCapturedResultReceived: (result) => {
                if (result.barcodeResultItems?.length) {
                    let value = result.barcodeResultItems[0].text;
                    console.log(target + ": " + value);
                    let inputControl = $("input[name='" + target + "']");
                    inputControl.val(value);
                    inputControl.focus();
                    inputControl.trigger("onchange");
                    cameraEnhancer.close();
                    barcodeScannerDiv.style.display = "none";

                }
            }
        });

        let filter = new Dynamsoft.Utility.MultiFrameResultCrossFilter();
        filter.enableResultCrossVerification("barcode", true);
        filter.enableResultDeduplication("barcode", true);
        await cvRouter.addResultFilter(filter);

        await cameraEnhancer.open();
        cameraView.setScanLaserVisible(true);
        await cvRouter.startCapturing("ReadSingleBarcode");
    })();
} // startScan

function addStyleClassToCameraDiv(barcodeScannerDiv) {
    barcodeScannerDiv.style.position = "fixed";
    barcodeScannerDiv.style.width = "100%";
    barcodeScannerDiv.style.height = "100%";
    barcodeScannerDiv.style.minWidth = "100px";
    barcodeScannerDiv.style.minHeight = "100px";
    barcodeScannerDiv.style.background = "rgb(221, 221, 221)";
    barcodeScannerDiv.style.left = "0px";
    barcodeScannerDiv.style.top = "0px";
    barcodeScannerDiv.style.display = "block";
}

