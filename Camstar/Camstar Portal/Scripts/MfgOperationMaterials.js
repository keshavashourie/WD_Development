// Copyright Siemens 2023

var mfgOperationMaterials = (function () {
    'use strict';

    var api = CR.GetNamespace('CR.MfgOperationMaterials');
    api.loadMaterialRequirements = loadMaterialRequirements;
    api.loadAttributes = loadAttributes;
    api.setLabels = setLabels;
    api.setGridLoadedHandler = setGridLoadedHandler;
    var _resourceDetailsComponents = [];
    var _resourceDetailsMaterialList = [];
    var _labels = [];
    var _gridLoadedHandler = null;

    var CTRL_IDS = {
        materialsGrid: 'mfgoperation-materials-grid'
    };
    Object.freeze(CTRL_IDS);

    var SELECTORS =
    {
        MATERIAL_REQ: "#mfgoperation-material-requirements",


        NO_DATA: '#mfgoperation-empty-state-materials',

        materialsGrid: '#' + CTRL_IDS.materialsGrid,
        materialsGridBody: "#mfgoperation-materials-grid > tbody",
        materialsGridBodyRows: "#mfgoperation-materials-grid > tbody tr",

        headerProduct: '#Web_Product',
        headerContainerLotQty: '#LblContainerQtyLot',
        headerQty: '#Container_Qty',
        headerSlot: '#ResourceSlots_Slot',
        headerSubSlot: '#ResourceSlots_SubSlot',
        headerProducingOrder: '#ProducingOrder',
        headerStatus: '#Status'
    };
    Object.freeze(SELECTORS);

    // interface

    var mfgOperationMaterialsInterface = {

        validateRequirementFulfill: validateRequirementFulfill,
        removeNonRequirement: removeNonRequirement

    }
    return mfgOperationMaterialsInterface;


    var MATERIAL_TEMPLATES = {
        materialRow: "<tr id='_TRID_' id='_#_' product='_PRODUCT_' islot='_ISLOT_' identifier='_IDENTIFIER_' slot-name='_SLOTNAME_' subslot-name='_SUBSLOTNAME_' qty='_QTY_' required-qty='_REQUIREDQTY_' from-material='_FROMMATERIAL_'>\
                 <td class='mfgoperation-col-150'>_PRODUCT_</td>\
                <td class='align-center'>\
                    <span class='icon-indicator'></span>\
                </td>\
                <td id='compInputCell_TRID_' class='mfgoperation-col-150'>\
                    <span id='_INPUTID_' value='_IDENTIFIER_' class='_IDSPANCLASS_' >_IDENTIFIER_</span>\
                </td>\
                <td>\
                    _QTY_\
                </td>\
                <td>\
                    _SLOTNAME_\
                </td>\
                <td>\
                    _PRODUCINGORDER_\
                </td>\
            </tr>"
    };
    Object.freeze(MATERIAL_TEMPLATES);

    var _mfgOrder = null;
    var _hideNonRequirement = false;

    function loadMaterialRequirements(resourceName, mfgOrder, hideNonRequirement) {
        _mfgOrder = mfgOrder;
        _hideNonRequirement = hideNonRequirement;
        if (mfgOrder && resourceName) {
            let request = {
                SpecId: mfgOrder.Spec,
                MfgOrderId: mfgOrder.MfgOrder,
                RouteStepId: mfgOrder.RouteStepId,
                ResourceName: resourceName
            };

            $.ajax({
                type: "POST",
                dataType: "json",
                url: './MfgOperationService.svc/web/LoadMfgOrderResourceMaterialRequirements',
                headers: {
                    'Accept': 'application/json'
                },
                contentType: "application/json;charset=UTF-8",
                async: false,
                data: JSON.stringify(request),
                context: document.body
            })
                .success(populateResourceDetails)
                .fail(null);
        }
    }

    // 
    function loadAttributes(resourceName, mfgOrder, successCallback) {

        if (!mfgOrder && !resourceName) {
            return;
        }

        let request = {
            mfgOrderId: mfgOrder.MfgOrder,
            productId: mfgOrder.Product,
            resourceName: resourceName
        };

        $.ajax({
            type: "POST",
            dataType: "json",
            url: './MfgOperationService.svc/web/LoadAttributes',
            headers: {
                'Accept': 'application/json'
            },
            contentType: "application/json;charset=UTF-8",
            async: false,
            data: JSON.stringify(request),
            context: document.body
        })
            .success(successCallback)
            .fail(null);
    }

    function setGridLoadedHandler(handler) {
        _gridLoadedHandler = handler;
    }

    function setLabels(labels) {
        _labels = labels;
        $(SELECTORS.headerProduct).text(_labels["Product"]);
        $(SELECTORS.headerContainerLotQty).text(_labels["ContainerLotQty"]);
        $(SELECTORS.headerQty).text(_labels["Quantity"]);
        $(SELECTORS.headerSlot).text(_labels["Slot"]);
        $(SELECTORS.headerSubSlot).text(_labels["SubSlot"]);
        $(SELECTORS.headerProducingOrder).text(_labels["ProducingOrder"]);
        $(SELECTORS.headerStatus).text(_labels["Status"]);
    }

    function populateResourceDetails(response) {

        clearGrid();
        _resourceDetailsComponents = [];
        _resourceDetailsMaterialList = [];
        if (!response.LoadMfgOrderResourceMaterialRequirementsResult.Success) {
            return;
        }

        else {
            if (response.LoadMfgOrderResourceMaterialRequirementsResult.Components)
                _resourceDetailsComponents = response.LoadMfgOrderResourceMaterialRequirementsResult.Components;

            if (response.LoadMfgOrderResourceMaterialRequirementsResult.MaterialList)
                _resourceDetailsMaterialList = response.LoadMfgOrderResourceMaterialRequirementsResult.MaterialList;
        }

        // Get the component & material list from response
        if (response) {
            document.getElementById('Slot') != null ? document.getElementById('Slot').innerHTML = 'Slot:SubSlot' : null;
            if (_resourceDetailsComponents.length > 0) {
                $(SELECTORS.NO_DATA).hide();
                for (var i = 0; i < _resourceDetailsComponents.length; i++) {
                    let name = _resourceDetailsComponents[i].Lot + "_" + _resourceDetailsComponents[i].Product + "_" + _resourceDetailsComponents[i].SlotName + "_" + _resourceDetailsComponents[i].SubSlotName;
                    _resourceDetailsComponents[i].RowId = name;
                    addRowToGrid(_resourceDetailsComponents[i].Lot + "_" + _resourceDetailsComponents[i].Product,
                        _resourceDetailsComponents[i].SlotName + "_" + _resourceDetailsComponents[i].SubSlotName, _resourceDetailsComponents[i].Product,
                        _resourceDetailsComponents[i].Lot, _resourceDetailsComponents[i].ProducingOrder, _resourceDetailsComponents[i].QtyRequired * _mfgOrder.MfgOrderQty, 0, true, _resourceDetailsComponents[i].SubSlotName);
                }
            }
            if (_resourceDetailsMaterialList.length > 0) {
                $(SELECTORS.NO_DATA).hide();
                for (var i = 0; i < _resourceDetailsMaterialList.length; i++) {
                    addRowToGrid(_resourceDetailsMaterialList[i].ContainerName + "_" + _resourceDetailsMaterialList[i].PartNumber,
                        "", _resourceDetailsMaterialList[i].PartNumber,
                        _resourceDetailsMaterialList[i].ContainerName, _resourceDetailsMaterialList[i].ProducingOrder, _resourceDetailsMaterialList[i].QtyRequired * _mfgOrder.MfgOrderQty, 0, true, _resourceDetailsMaterialList[i].SubSlotName);
                }

            }
            validateRequirementFulfill(_resourceDetailsComponents, _resourceDetailsMaterialList);
            if (_hideNonRequirement)
                removeNonRequirement();
        }

        else
            $(SELECTORS.NO_DATA).show();

        if (_gridLoadedHandler) {
            _gridLoadedHandler(CTRL_IDS.materialsGrid);
        }
    }


    function getElement(idOrSelector) {
        // if it's not a selector, assume it's an ID an prepend a '#'
        var selector = (idOrSelector[0] === '#' || idOrSelector[0] === '.') ? idOrSelector : '#' + idOrSelector;

        // return value
        var $elems = $(selector);

        // if not found, check parent
        if ($elems.length === 0 && parent)
            $elems = $(selector, parent.document);

        // if not found, check first iframe in document (?)
        if ($elems.length === 0) {
            var iframe = $($(document).find('iframe')[0]);
            if (iframe.length === 1)
                $elems = iframe.contents().find(selector);
        }
        return $elems;
    }

    function addRowToGrid(rowId, slotName, productName, identifier, producingOrder, qty, qtyRequired, isLot, subSlotName) {
        if (typeof MATERIAL_TEMPLATES === 'undefined') {
            var MATERIAL_TEMPLATES = {
                materialRow: "<tr id='_TRID_' id='_#_' product='_PRODUCT_' islot='_ISLOT_' identifier='_IDENTIFIER_' slot-name='_SLOTNAME_' subslot-name='_SUBSLOTNAME_' qty='_QTY_' required-qty='_REQUIREDQTY_' from-material='_FROMMATERIAL_'>\
                 <td class='mfgoperation-col-150'>_PRODUCT_</td>\
                <td class='align-center'>\
                    <span class='icon-indicator'></span>\
                </td>\
                <td id='compInputCell_TRID_' class='mfgoperation-col-150'>\
                    <span id='_INPUTID_' value='_IDENTIFIER_' class='_IDSPANCLASS_' >_IDENTIFIER_</span>\
                </td>\
                <td>\
                    _QTY_\
                </td>\
                <td>\
                    _SLOTNAME_\
                </td>\
                <td>\
                    _PRODUCINGORDER_\
                </td>\
            </tr>"
            };
            Object.freeze(MATERIAL_TEMPLATES);

            let table = getElement(SELECTORS.materialsGrid);
            let rowNum = table.find("tbody > tr").length + 1;
            //making a unqiue id for css replace later
            rowId = rowId + "_" + slotName;
            var editedslotName = slotName.replace(new RegExp('_', 'g'), ':');
            var newRowMarkup = MATERIAL_TEMPLATES.materialRow
                .replace(new RegExp('_TRID_', 'g'), rowId)
                .replace(new RegExp('_#_', 'g'), rowNum)
                .replace(new RegExp('_PRODUCT_', 'g'), productName ? productName : '')
                .replace(new RegExp('_IDENTIFIER_', 'g'), identifier ? identifier : '')
                .replace(new RegExp('_SLOTNAME_', 'g'), slotName ? editedslotName : '')
                .replace(new RegExp('_QTY_', 'g'), qty ? qty : '')
                .replace(new RegExp('_REQUIREDQTY_', 'g'), qtyRequired ? qtyRequired : '')
                .replace(new RegExp('_FROMMATERIAL_', 'g'), 'false')
                .replace(new RegExp('_ISLOT_', 'g'), isLot ? 'true' : 'false')
                .replace(new RegExp('_PRODUCINGORDER_', 'g'), producingOrder ? producingOrder : '')
                ;

            let $newRow = $(newRowMarkup);
            $newRow.find('span.icon-indicator').addClass('requirement-not-loaded-icon').attr('title', _labels['NotLoaded']);
            getElement(SELECTORS.materialsGridBody).append($newRow);
            return rowNum;
        }
        else {
            let table = getElement(SELECTORS.materialsGrid);
            let rowNum = table.find("tbody > tr").length + 1;
            //making a unqiue id for css replace later
            rowId = rowId + "_" + slotName + "_" + subSlotName;
            var newRowMarkup = MATERIAL_TEMPLATES.materialRow
                .replace(new RegExp('_TRID_', 'g'), rowId)
                .replace(new RegExp('_#_', 'g'), rowNum)
                .replace(new RegExp('_PRODUCT_', 'g'), productName ? productName : '')
                .replace(new RegExp('_IDENTIFIER_', 'g'), identifier ? identifier : '')
                .replace(new RegExp('_SLOTNAME_', 'g'), slotName ? slotName : '')
                .replace(new RegExp('_QTY_', 'g'), qty ? qty : '')
                .replace(new RegExp('_REQUIREDQTY_', 'g'), qtyRequired ? qtyRequired : '')
                .replace(new RegExp('_FROMMATERIAL_', 'g'), 'false')
                .replace(new RegExp('_ISLOT_', 'g'), isLot ? 'true' : 'false')
                .replace(new RegExp('_PRODUCINGORDER_', 'g'), producingOrder ? producingOrder : '')
                ;

            let $newRow = $(newRowMarkup);
            $newRow.find('span.icon-indicator').addClass('requirement-not-loaded-icon').attr('title', _labels['NotLoaded']);
            getElement(SELECTORS.materialsGridBody).append($newRow);
            return rowNum;
        }
    }

    function clearGrid() {
        $(SELECTORS.materialsGridBody).html("");
    }

    function validateRequirementFulfill(components, materialList) {
        let fromMaterial = false;
        let cssClass = "";
        components = components.concat(materialList);

        for (var index in components) {
            let comp = components[index];
            let title = '';
            let $compRow = $(SELECTORS.materialsGridBody).find('tr[id="' + comp.RowId + '"]');
            if (comp.Product && comp.Product !== materialList.PartNumber && !comp.Satisfied) {
                cssClass = "not-required";
                title = _labels['NotRequired'];
                fromMaterial = true;
                $compRow.attr('from-material', fromMaterial);
            }
            else if (comp.Satisfied) {
                cssClass = "requirement-fulfill";
                title = _labels['Satisfied'];
                fromMaterial = false;
                $compRow.attr('from-material', fromMaterial);
            }
            else {
                fromMaterial = false;
                cssClass = "";
                $compRow.attr('from-material', fromMaterial);
            }
            let icon = $compRow.find('span.icon-indicator');
            icon.removeClass('requirement-not-loaded-icon');
            icon.addClass(cssClass + '-icon').attr('title', title);

        }

    }

    function removeNonRequirement() {
        let $removeRow = $(SELECTORS.materialsGridBody).find('tr[from-material="true"]');
        $removeRow.remove();
    }

})();
