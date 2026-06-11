// Copyright 2024 Siemens
// Shared between 3 modeling pages that can be used to import BOM items
var BomItemImportVP = (function () {
    'use strict';

    var bomMaintInterface = {
        initialize: initialize
    };

    var SELECTORS = {
        PICK_IMPORT_FILE: 'input#pick-import-file',
        IMPORT_BOM_ITEMS: 'input[name$="$ImportBomItems"]',
        UPDATE_BOM_ITEMS: 'input[name$="$UpdateBomItems"]',
        OPEN_IMPORT_POPUP: 'input[name$="OpenImportPopup"]',
        BOM_ITEMS_JSON: 'input[name$="$BomItemsJson$ctl00"]',
        IMPORT_FILE_CONTENT_JSON: 'input[name$="ImportFileContentJson$ctl00"]',
        APPLY_FILTER: 'input[id$="ApplyFilter"]',
        FILTER_VALUE: 'span[id$="_FilterValue"]',
        FILTER_VALUE_INPUT: 'input[id$="_FilterValue_ctl00"]'
    };

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

    // 
    function updateBomItemsClick(e) {
        // tell user to save if they have made changes
        if (__page.isDirty()) {
            CR.Event.stopEvent(e);

            var labels = [{ Name: 'Lbl_MustSaveBeforeUpdate' }, { Name: 'Lbl_Warning' }];
            __page.getLabels(labels, function (response) {
                if (Array.isArray(response)) {
                    var message = response.find(l => l.Name === 'Lbl_MustSaveBeforeUpdate').Value;
                    var warning = response.find(l => l.Name === 'Lbl_Warning').Value;

                    $.alerts.alert(message, warning);
                }
                else {
                    alert(response.Error);
                }
            });
        }
    }

    let filterTimeoutId = null;
    function filterValueChanged() {
        clearTimeout(filterTimeoutId);

        filterTimeoutId = setTimeout(function () {
            // disable any more input during postback
            $(SELECTORS.FILTER_VALUE_INPUT).prop('disabled', true);
            $(SELECTORS.APPLY_FILTER).trigger("click");
        }, 400);
    }

    // after a postback the focus will be in the "Filter Value" text box
    // but the text will be selected.  This removes the selection and puts
    // the cursor at the end
    function filterValueFocused() {
        let filterInput = this;

        // re-enable input (after postback done)
        $(SELECTORS.FILTER_VALUE_INPUT).prop('disabled', false);

        let valLen = $(SELECTORS.FILTER_VALUE_INPUT).val().length;

        setTimeout(function () {
            filterInput.setSelectionRange(valLen, valLen);
        }, 0);
    }

    // triggered from server side
    function initialize() {
        $(SELECTORS.IMPORT_BOM_ITEMS).click(importBomItemsClick);
        $(SELECTORS.UPDATE_BOM_ITEMS).click(updateBomItemsClick);
        $(SELECTORS.FILTER_VALUE_INPUT).on("input paste", filterValueChanged);
        $(SELECTORS.FILTER_VALUE_INPUT).on("focus", filterValueFocused);

        // TODO - no inline style!
        $(SELECTORS.IMPORT_BOM_ITEMS).after('<input id="pick-import-file" name="pick-import-file" type="file" accept=".csv" value="XImport ItemsX" class="cs-button" formnovalidate>');
        $(SELECTORS.PICK_IMPORT_FILE).change(importFilePicked);
    }

    return bomMaintInterface;
})();