// Copyright 2024 Siemens
var BomMaint = (function () {
    'use strict';

    var bomMaintInterface = {
        initialize: initialize
    };

    var SELECTORS = Object.freeze({
        PICK_IMPORT_FILE: 'input#pick-import-file',
        IMPORT_BOM_ITEMS: '#ctl00_WebPartManager_MaterialsGroupWP_ImportBomItems',
        OPEN_IMPORT_POPUP: '#ctl00_WebPartManager_MaterialsGroupWP_OpenImportPopup',
        BOM_ITEMS_JSON: 'input#ctl00_WebPartManager_MaterialsGroupWP_BomItemsJson_ctl00',
        IMPORT_FILE_CONTENT_JSON: 'input#ctl00_WebPartManager_MaterialsGroupWP_ImportFileContentJson_ctl00'
    });

    // file picked, can now read it and pass contents to popup
    function importFilePicked(changeEvent) {
        const bomItemFile = changeEvent.target.files[0];
        if (!bomItemFile)
            return;

        // read contents of BOM item .csv file
        const importFileReader = new FileReader();

        importFileReader.onload = function () {

            // parse
            let importFileContent = d3.csv.parseRows(importFileReader.result);

            // set data to be sent to preview popup through DataContractLink
            $(SELECTORS.IMPORT_FILE_CONTENT_JSON).val(JSON.stringify(importFileContent));

            // open import popup
            $(SELECTORS.OPEN_IMPORT_POPUP).click();

            // clear value so we can import again
            $(SELECTORS.PICK_IMPORT_FILE).val('');
        };

        importFileReader.onerror = function () {
            __page.displayStatus(importFileReader.error, "Error");
        };

        importFileReader.readAsText(bomItemFile);
    }

    // click handler to start the import process
    function importBomItemsClick(e) {
        CR.Event.stopEvent(e);

        // let user choose file, then open popup
        $(SELECTORS.PICK_IMPORT_FILE).click();
    }

    // triggered from server side
    function initialize() {
        $(SELECTORS.IMPORT_BOM_ITEMS).click(importBomItemsClick);

        // TODO - no inline style!
        $(SELECTORS.IMPORT_BOM_ITEMS).after('<input id="pick-import-file" name="pick-import-file" type="file" accept=".csv" value="XImport ItemsX" class="cs-button" formnovalidate>');
        $(SELECTORS.PICK_IMPORT_FILE).change(importFilePicked);
    }

    return bomMaintInterface;
})();