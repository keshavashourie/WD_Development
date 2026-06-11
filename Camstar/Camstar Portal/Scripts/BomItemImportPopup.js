var BomItemImportPopup = (function () {
    'use strict';

    var bomItemImportPopupInterface = {
        initialize: initialize
    };

    var SELECTORS = Object.freeze({
        BOM_ITEMS_JSON: '#ctl00_WebPartManager_BomItemImportWP_BomItemsJson_ctl00',
        BOM_IMPORT_CONFIG_JSON: '#ctl00_WebPartManager_BomItemImportWP_BomImportConfigJson_ctl00',
        IMPORT_FILE_CONTENT_JSON: '#ctl00_WebPartManager_BomItemImportWP_ImportFileContentJson_ctl00',
        PREVIEW_TABLE_CONTAINER: '#preview-table-container',
        PREVIEW_TABLE: '#preview-table',
        PREVIEW_TABLE_HEAD: 'table#preview-table > thead',
        PREVIEW_TABLE_BODY: 'table#preview-table > tbody',
        PREVIEW_HEADER_SELECTS: 'table#preview-table > thead select',
        IMPORT: '#import',
        SERVER_IMPORT_BUTTON: '#ctl00_WebPartManager_ButtonsBar_CustomActionImport',
        DEFAULT_ISSUE_CONTROL: '#ctl00_WebPartManager_BomItemImportWP_DefaultIssueControl_Value',
        DEFAULT_QUANTITY: '#ctl00_WebPartManager_BomItemImportWP_DefaultQuantity_ctl00',
        DEFAULT_PRODUCT_REVISION: '#ctl00_WebPartManager_BomItemImportWP_DefaultProductRevision_ctl00',
        DEFAULT_BOM_REVISION: '#ctl00_WebPartManager_BomItemImportWP_DefaultBomRevision_ctl00',
        DEFAULT_SPEC_REVISION: '#ctl00_WebPartManager_BomItemImportWP_DefaultSpecRevision_ctl00',
        IMPORT_MULTIPLE_BOMS: '#ctl00_WebPartManager_BomItemImportWP_ImportMultipleBoms',
        IMPORT_MULTIPLE_BOMS_LABEL: '#ctl00_WebPartManager_BomItemImportWP_ImportMultipleBoms > span.cs-label'
    });

    // TODO - localize text
    let COL_MAP_OPTIONS = [
        {
            columnId: 'RefDes',
            text: '*Ref Des',
            labelName: 'csiRefDes'
        },
        {
            columnId: 'ProductName',
            text: '*Product Name',
            labelName: 'ContainerStatusDetails_ProductName'
        },
        {
            columnId: 'ProductRevision',
            text: '*Product Revision',
            labelName: 'ResourceStatusDetails_ProductRev'
        },
        {
            columnId: 'Quantity',
            text: '*Quantity',
            labelName: 'Web_Quantity'
        },
        {
            columnId: 'IssueControl',
            text: '*Issue Control',
            labelName: 'Lbl_IssueControl'
        },
        {
            columnId: 'SpecName',
            text: '*Spec Name',
            labelName: 'Spec_Name'
        },
        {
            columnId: 'SpecRevision',
            text: '*Spec Revision',
            labelName: 'ContainerStatusDetails_SpecRevision'
        },
        {
            columnId: 'RouteStep',
            text: '*Route Step',
            labelName: 'Step_RouteStep'
        },
        {
            columnId: 'AllowUnderConsumption',
            text: '*Allow Under Consumption',
            labelName: 'MaterialListItem_AllowUnderConsumption'
        },
        {
            columnId: 'AllowOverConsumption',
            text: '*Allow Over Consumption',
            labelName: 'MaterialListItem_AllowOverConsumption'
        },
        {
            columnId: 'BomName',
            text: 'BOM Name',
            labelName: 'WebUI_BomName'
        },
        {
            columnId: 'BomRevision',
            text: 'BOM Revision',
            labelName: 'WebUI_BomRevision'
        }
    ];

    // hang onto this to save with config later
    let _additionalColumnOptions = [];
    let _enumMaps = [];

    let LABELS = 
    {
        Lbl_FieldIsRequired: '*Field {0} is required<br>',
        WebUI_DefaultQuantity: '*Quantity',
        WebUI_DefaultProductRevision: '*Default Product Revision',
        WebUI_DefaultIssueControl: '*Default Issue Control',
        WebUI_DefaultBomRevision: '*Default BOM Revision',
        Lbl_Error: 'Error',
        MustDesignateAProductColumn: '*You Must Designate A Product Column',
        WebUI_ImportMultipleBomsXDetected: '*Import Multiple BOMs ({0} detected)'
    };

    // triggered from server
    function initialize(colMaps, additionalColumnOptions, enumMaps) {
        $(SELECTORS.SERVER_IMPORT_BUTTON).click(importClick);

        // get BOM items to import from data contract-linked text field
        let importContent = JSON.parse($(SELECTORS.IMPORT_FILE_CONTENT_JSON).val());

        _additionalColumnOptions = JSON.parse(additionalColumnOptions);
        COL_MAP_OPTIONS = COL_MAP_OPTIONS.concat(_additionalColumnOptions);

        _enumMaps = JSON.parse(enumMaps);

        // add header row
        let $headerRow = $('<tr></tr>');
        for (let i = 0; i < importContent[0].length; i++) {
            let $select = $(`<select id='${'import-header-select-' + i}'></select>`);
            addOptions($select, []);
            $select.on('change', onColMapChange);
            let $th = $('<th></th>');
            $th.append($select);
            $headerRow.append($th);
        }
        $(SELECTORS.PREVIEW_TABLE_HEAD).append($headerRow);

        // add row for each BOM item
        importContent.forEach(item => {
            let $tr = $('<tr></tr>');

            // each property on the BOM item object
            item.forEach(prop => {
                $tr.append(`<td>${prop}</td>`);
            });

            $(SELECTORS.PREVIEW_TABLE_BODY).append($tr);
        });

        colMaps = JSON.parse(colMaps);

        if (colMaps) {
            colMaps.forEach(colMap => {

                $(SELECTORS.PREVIEW_HEADER_SELECTS)
                    .eq(colMap.index)                               // get column header <select> at index
                    .find(`option[value="${colMap.columnId}"]`)     // find the <option> indicating what property is stored in this column
                    .prop('selected', true);                        // select it
            });
        }

        // col mapping
        let labelNames = COL_MAP_OPTIONS
            .map(cmo => cmo.labelName)
            .filter(labelName => labelName);        // exclude any falsy (blank) label names

        // other labels (error messages)
        for (const labelName in LABELS) {
            labelNames.push(labelName);
        }
        CR.Page.localizeLabels(labelNames, localizeLabelsSuccess, false);
    }

    // done loading
    function localizeLabelsSuccess(labels) {
        labels.forEach(label => {
            // find colum map option that needs this label
            let cmo = COL_MAP_OPTIONS.find(cmo => cmo.labelName === label.Name);
            if (cmo) {
                cmo.text = label.Value;
            }

            // labels object
            if (LABELS.hasOwnProperty(label.Name)) {
                LABELS[label.Name] = label.Value;
            }
        });

        // option text has been localized - ok to add to <select>s
        setHeaderSelectOptions();
        setSubBomCount();
    }

    // 
    function onColMapChange() {
        setHeaderSelectOptions();
        setSubBomCount();
    }

    // re-do options for all selects - do not include any options that are already assigned to other columns
    function setHeaderSelectOptions() {
        let selectedValues =
            $(SELECTORS.PREVIEW_HEADER_SELECTS)
                .map(function (index, elem) {
                    return elem.value === 'skip' ? undefined : elem.value;
                })
                .get();

        // iterate over all the <select> elems in the table header
        $(SELECTORS.PREVIEW_HEADER_SELECTS).each(function (index, selectElem) {
            let $selectElem = $(selectElem);
            $selectElem.off('change');  // remove so it doesn't trigger change events
            let selectedValue = selectElem.value;

            // remove options
            $selectElem.html('');

            // add all options that aren't selected
            let otherSelectedValues = selectedValues.filter(val => val !== selectedValue);
            addOptions($selectElem, otherSelectedValues);

            // reselect the value that was selected
            selectElem.value = selectedValue;

            // re-add change handler
            $selectElem.on('change', onColMapChange);
        });
    }

    // Update label indicating how many sub (child) BOMs are in the file
    function setSubBomCount() {
        // any column designated as the "BOM Name"?
        let columnMaps = getColumnMaps();
        let bomNameCol = columnMaps.find(cm => cm.columnId === 'BomName');

        let subBomCount = 0;
        if (bomNameCol) {
            let bomItems = getBomItems(columnMaps, true);

            // count distinct BOMs
            let distinctBoms = [];
            bomItems.forEach(bi => {
                let bomNameRev = bi.BomName + bi.BomRevision;
                if (bi.BomName && !distinctBoms.includes(bomNameRev))
                    distinctBoms.push(bomNameRev);
            });

            subBomCount = distinctBoms.length;
        }

        if (subBomCount === 0) {
            $(SELECTORS.IMPORT_MULTIPLE_BOMS).attr('checked', null).attr('disabled', 'disabled');
        } else {
            $(SELECTORS.IMPORT_MULTIPLE_BOMS).attr('disabled', null);
        }

        $(SELECTORS.IMPORT_MULTIPLE_BOMS_LABEL).text(LABELS.WebUI_ImportMultipleBomsXDetected.replace("{0}", subBomCount));
    }

    // $select - <select> object to add <options> to
    // excludeValues - do NOT add values that are in this array
    function addOptions($select, excludeValues) {
        // every <select> has the skip option
        // TODO - localize
        $select.append($(`<option value='${'skip'}'>${'Skip'}</option>`));

        COL_MAP_OPTIONS.forEach(option => {
            if (!excludeValues.includes(option.columnId)) {
                $select.append($(`<option value='${option.columnId}'>${option.text}</option>`));
            }
        });
    }

    // client-side defined button click handler - set BOM items to hidden text field and trigger server processing
    function importClick(ev) {
        // validate defaults
        let defaultQuantity = $(SELECTORS.DEFAULT_QUANTITY).val();
        let defaultProductRevision = $(SELECTORS.DEFAULT_PRODUCT_REVISION).val();
        let defaultIssueControl = $(SELECTORS.DEFAULT_ISSUE_CONTROL).val();
        let defaultBomRevision = $(SELECTORS.DEFAULT_BOM_REVISION).val();
        let defaultSpecRevision = $(SELECTORS.DEFAULT_SPEC_REVISION).val();
        

        if (!defaultQuantity) {
            __page.displayStatus(LABELS.Lbl_FieldIsRequired.replace('{0}', LABELS.WebUI_DefaultQuantity), LABELS.Lbl_Error);
            return false;
        }

        if (!defaultProductRevision) {
            __page.displayStatus(LABELS.Lbl_FieldIsRequired.replace('{0}', LABELS.WebUI_DefaultProductRevision), LABELS.Lbl_Error);
            return false;
        }

        if (!defaultIssueControl) {
            __page.displayStatus(LABELS.Lbl_FieldIsRequired.replace('{0}', LABELS.WebUI_DefaultIssueControl), LABELS.Lbl_Error);
            return false;
        }

        if (!defaultBomRevision) {
            __page.displayStatus(LABELS.Lbl_FieldIsRequired.replace('{0}', LABELS.WebUI_DefaultBomRevision), LABELS.Lbl_Error);
            return false;
        }

        if (!defaultSpecRevision) {
            __page.displayStatus(LABELS.Lbl_FieldIsRequired.replace('{0}', LABELS.WebUI_DefaultSpecRevision), LABELS.Lbl_Error);
            return false;
        }

        // build column map - use naming conventions expected by server
        let columnMaps = getColumnMaps();

        // make sure user has picked a Product column
        let prodCol = columnMaps.find((cm) => {
            return cm.columnId === 'ProductName';
        });

        if (!prodCol) {
            __page.displayStatus(LABELS.MustDesignateAProductColumn, LABELS.Lbl_Error);
            return false;
        }

        let importMultipleBoms = $(SELECTORS.IMPORT_MULTIPLE_BOMS).attr('checked') === 'checked';
        let bomItems = getBomItems(columnMaps, importMultipleBoms);

        $(SELECTORS.BOM_ITEMS_JSON).val(JSON.stringify(bomItems));

        // set import config so server can save it
        let importConfig = {
            columnMaps: columnMaps,
            additionalColumnOptions: _additionalColumnOptions,
            enumMaps: _enumMaps,
            defaultIssueControl: defaultIssueControl,
            defaultQuantity: defaultQuantity,
            defaultProductRevision: defaultProductRevision,
            defaultBomRevision: defaultBomRevision,
            defaultSpecRevision: defaultSpecRevision,
            importMultipleBoms: importMultipleBoms
        };

        $(SELECTORS.BOM_IMPORT_CONFIG_JSON).val(JSON.stringify(importConfig));

        return true;
    }

    // build column map - use naming conventions expected by server
    function getColumnMaps() {
        let columnMaps =
            $(SELECTORS.PREVIEW_HEADER_SELECTS)
                .map(function (index, elem) {
                    return {
                        'index': index,
                        'columnId': elem.value
                    };
                })
                .get();

        return columnMaps;
    }

    // apply map to data from the import file (2D array)
    function getBomItems(columnMaps, includeMultipleBoms = false) {
        let importContent = JSON.parse($(SELECTORS.IMPORT_FILE_CONTENT_JSON).val());

        let bomItems =
            importContent.map(function (item) {
                let bomObj = {};

                columnMaps.forEach(colMap => {
                    if (colMap.columnId !== 'skip') {
                        bomObj[colMap.columnId] = item[colMap.index].trim();

                        // translate value if there is an enum map
                        let enumMap = _enumMaps.find(em => em.columnId === colMap.columnId);
                        if (enumMap) {
                            let valueMap = enumMap.valueMaps.find(vm => vm.fromValue === bomObj[colMap.columnId]);
                            if (valueMap) {
                                bomObj[colMap.columnId] = valueMap.toValue;
                            }
                        }
                    }

                });

                return bomObj;
            });

        // remove any child BOMs if we don't want them
        if (!includeMultipleBoms) {
            bomItems = bomItems.filter(bi => !bi.BomName);
        }

        let defaultQuantity = $(SELECTORS.DEFAULT_QUANTITY).val();
        let defaultProductRevision = $(SELECTORS.DEFAULT_PRODUCT_REVISION).val();
        let defaultIssueControl = $(SELECTORS.DEFAULT_ISSUE_CONTROL).val();
        let defaultBomRevision = $(SELECTORS.DEFAULT_BOM_REVISION).val();
        let defaultSpecRevision = $(SELECTORS.DEFAULT_SPEC_REVISION).val();

        // apply defaults
        bomItems.forEach(bi => {
            bi.Quantity = bi.Quantity ?? defaultQuantity;
            bi.IssueControl = bi.IssueControl ?? defaultIssueControl;
            bi.ProductRevision = bi.ProductRevision || defaultProductRevision;
            if (bi.SpecName) {
                bi.SpecRevision = bi.SpecRevision || defaultSpecRevision;
            }
            if (bi.BomName) {
                bi.BomRevision = bi.BomRevision || defaultBomRevision;
            }
        });

        return bomItems;
    }
    return bomItemImportPopupInterface;
})();