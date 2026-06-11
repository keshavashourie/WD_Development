// Copyright Siemens 2024  

var mfgOperation = (function () {
    'use strict';

    var mfgOrderList = [];          // represents list of orders found for selected resource. properties match code behind MfgOrderListItem class
    var labels = {};                // labels for use on the page
    var displayDetails = [];        // array of objs defining properites to display in Mfg Order Detail panel. 
    var orderClickHandlers = [];    // any handlers in here will get called when a tile is clicked.
    var selectedOrder = null;
    var documents = null;
    var _sideBarInitialized = false;
    var _rszTime = null;
    var _firstResizeDone = false;
    var _contentHeightAdjust = 38;

    var TEMPLATE = {
        contentRow: '<div class="content-row">\
                        <span class="content-name">_CONTENT_ROW_NAME_</span>\
                        <span class="content-value">_CONTENT_ROW_VALUE_</span>\
                    </div>'
    };

    var CTRL_IDS = {
        MATERIALS_GRID_CONTAINER: 'mfgoperation-materials-grid-container',
        MATERIALS_GRID: 'mfgoperation-materials-grid'
    };
    Object.freeze(CTRL_IDS);

    var SELECTOR = {
        TILE_LIST: '#ctl00_WebPartManager_MfgOperationWP_MfgOrderList .item',
        DETAIL_HEADER: '#mfgoperation-order-detail-header',
        DETAIL_HEADER_TITLE: '#mfgoperation-order-detail-header > .title',
        DETAIL_CONTENT: '#mfgoperation-order-detail-content',
        DETAIL_CONTENT_TABLE: '#mfgoperation-order-detail-content > .content-tbl',
        DETAIL_CONTAINER: "#mfgOperation-details-container",
        MATERIALS: "#mfgoperation-material",
        MATERIALS_CONTENT: "#mfgoperation-material-content",
        MATERIALS_HEADER: "#mfgoperation-material-header",
        HIDE_NON_REQUIREMENT: '#mfgoperation-hide-non-requirement-span',
        HIDE_NON_REQUIREMENT_LABEL: '#lbl-mfgoperation-hide-non-requirement',
        RESOURCE_HEADER: '#WebPart_ResourceHeader_WP',
        RESOURCE_NAME: '#ctl00_WebPartManager_ResourceHeader_WP_Resource_Edit',
        MAIN: "#mfgoperation-main",
        DETAILS: '#ctl00_WebPartManager_MfgOperationWP_MfgOrderDetail',
        CONTENT: '#mfgoperation-content',
        MATERIAL_REQ: "#mfgoperation-material-requirements",
        MATERIALS_MAX: '#material-maximize',
        NO_DATA: '#mfgoperation-emp-state',
        NO_DATA_TEXT: '.mfgoperation-empty-state-txt',
        NO_DATA_IMG: '#mfgoperation-empty-state-img',

        MATERIALS_GRID: '#' + CTRL_IDS.MATERIALS_GRID,
        WORKFLOW_STEP: '#ctl00_WebPartManager_MfgOperationWP_WorkflowStep_ctl00',
        MFGOP_WEBPART_MATRIX_ROW: '#WebPart_MfgOperationWP > div.matrix > div.row',
        TEMPLATE_CONTENT_DIV_ROW: '#TemplateContentDiv > div.container-fluid > div.row'
    };

    // interface
    var mfgOperationInterface = {
        addOrderTileClickHandler: addOrderTileClickHandler,
        replaceOrderTileClickHandler: replaceOrderTileClickHandler,
        haveData: haveData,
        initialize: initialize,
        setDisplayDetails: setDisplayDetails,
        setOrderList: setOrderList,
        rebuildDetails: rebuildDetails,
        resizeHandler: null,
        detailsRebuiltHandler: null,
        showDocuments: showDocuments,
        showAttributes: showAttributes,
        setCheckBoxChecked: setCheckBoxChecked
    }
    return mfgOperationInterface;

    // Sets the properties to display in the Mfg Order Details panel
    // set extend = true to add properites and false to show only the specified properties.
    // details is an array of objects of the form { label : <labelName>, value : <orderPropName> }
    // - label is the name of the property in the "labels" obj to use as the property name.
    // - value is the name of the property on the selected order to use as display value. 
    //         names correspond to property names of the code behind class MfgOrderListItem or of a class derived from it.
    // }
    function setDisplayDetails(extend, details) {
        if (extend)
            displayDetails = displayDetails.concat(details);
        else
            displayDetails = details;
    }

    // handler is a callback function
    // this will be called after the standard tile click handler executes to update the details.
    // the object corresponding to the clicked tile is passed as an argument to the callback.
    function addOrderTileClickHandler(handler) {
        let exists = null;
        orderClickHandlers.some(function (ord) {
            if (ord === handler) {
                exists = ord;
                return true;
            }
        });
        if (!exists)
            orderClickHandlers.push(handler);
    }
    function replaceOrderTileClickHandler(handler) {
        orderClickHandlers = [];
        addOrderTileClickHandler(handler);
    }
    function initialize(labelObj, displayDetailList) {
        labels = labelObj;
        displayDetails = displayDetailList;
        orderClickHandlers = [];
        selectedOrder = null;

        hideEverything();   //hide things so we don't see a flash on the page until ready to show
        setupResizeHandler();
        
        $(SELECTOR.DETAIL_HEADER_TITLE).text(labels.MfgOrderDetail);
        $(SELECTOR.NO_DATA_TEXT).html(labels["NoData"]);
        $(SELECTOR.MATERIAL_REQ).text(labels["MaterialRequirements"]);
        $(SELECTOR.HIDE_NON_REQUIREMENT_LABEL).text(labels["HideNonRequirement"]);

        //  Hide the cell that would hold the Resource Group dropdown
        let header = $(SELECTOR.RESOURCE_HEADER);
        header.find('div.row').children().first().hide();

        CR.MfgOperationMaterials.setLabels(labels);
        CR.Grid.makeScrollable(CTRL_IDS.MATERIALS_GRID, CTRL_IDS.MATERIALS_GRID_CONTAINER, false);
        CR.MfgOperationMaterials.setGridLoadedHandler(gridLoadedHandler);
        $(SELECTOR.HIDE_NON_REQUIREMENT).on('change', toggleHideNonRequirement);

        //after adding mfg order dropdown, the resource label position is not good. need to adjust its position
        var $resourceLabel = $('#ctl00_WebPartManager_ResourceHeader_WP_Resource').children().first();
        var currOffset = $resourceLabel.offset();
        currOffset.top = 24;
        $resourceLabel.offset(currOffset);
        $resourceLabel.css('padding-left', '4px');

        //remove horizontal scroll due to -15 margins
        $(SELECTOR.MFGOP_WEBPART_MATRIX_ROW).css({ "margin-left": 0, "margin-right": 0 });
        $(SELECTOR.TEMPLATE_CONTENT_DIV_ROW).css({ "margin-left": 0, "margin-right": 0 });

        setTimeout(function () {
            buildCommandBar();
            // to avoid screen flash, don't call updateDisplay() until after first screen resize. 
        }, 100);
    }

    function gridLoadedHandler(gridId) {
        // Resize the page after a grid loads. The exact size of the grid will not
        // be accurate until a grid has fully loaded its data.
        doResize();
    }

    function buildCommandBar() {
        var visible = mfgOperation.haveData();
        if (!_sideBarInitialized) {
            _sideBarInitialized = true;

            CR.SideBar.beginCustomize();
            CR.SideBar.addMenuItem('buttonDocuments', labels['Documents'], null, 'cr-button-documents', showDocuments, false, visible, 0);
            CR.SideBar.addMenuItem('buttonAttributes', labels['Attributes'], null, 'cr-button-attributes', showAttributes, false, true /*selectedOrder*/, 0);
            CR.SideBar.endCustomize();
        }
    }

    // 
    function showAttributes() {

        if (selectedOrder) {
            CR.MfgOperationMaterials.loadAttributes($(SELECTOR.RESOURCE_NAME).val(), selectedOrder, showAttributesSlideout);
        }
    }

    // Row for the name/value of the given attribute
    function getAttributeRowMarkup(attr) {
        let rowsMarkup =
            `<tr>
                <td>${attr.Key}</td>
                <td>${attr.Value}</td>
             </tr>`;

        return rowsMarkup;
    }

    // get section header and table of attributes
    function getAttributesMarkup(attrs, headerText, containerId) {
        let markup = '';
        if (attrs?.length) {
            let headerMarkup =
            `<thead>
                <tr class="ui-jqgrid-labels">
                    <th>${labels['Lbl_Name']}</th>
                    <th>${labels['SelVal_Value']}</th>
                </tr>
            </thead>`;


            let rowsMarkup = '';
            attrs.forEach(function (attr) {
                rowsMarkup += getAttributeRowMarkup(attr);
            });

            markup =
                `<div id="${'grid-container-' + containerId}" class="mfgoperation-shopfloor-grid-container cr-no-margin">
                    <div class="ui-jqgrid-title grid-label">${headerText}</div>
                    <div class="mfgoperation-grid-view">
                        <table class="mfgoperation-shopfloor-grid">
                            ${headerMarkup}
                            <tbody>${rowsMarkup}</tbody>
                        </table>
                    </div>
                </div>`;

        }
        return markup;
    }

    // Open and populate slideout with all attributes
    function showAttributesSlideout(response) {

        let $contentDiv = $('div#TemplateContentDiv');

        // mfg order
        let mfgOrderAttrsMarkup = getAttributesMarkup(response?.LoadAttributesResult?.MfgOrderAttributes, labels["MfgOrderName"], 'mfg-order');

        // product
        let productAttrsMarkup = getAttributesMarkup(response?.LoadAttributesResult?.ProductAttributes, labels["Product"], 'product');

        // resource
        let resourceAttrsMarkup = getAttributesMarkup(response?.LoadAttributesResult?.ResourceAttributes, labels["Web_Resource"], 'resource');

        let attrMarkup = `${mfgOrderAttrsMarkup}${productAttrsMarkup}${resourceAttrsMarkup}`;

        let $attrs = $(attrMarkup);

        CR.SlideOut.open(
            $contentDiv,
            'attributes-slide',       // TODO - using this for styling.  Have to update Core LESS
            labels['Attributes'],
            //TEMPLATE.slideOut,
            $attrs,
            '480px',
            'right',
            null,
            null,
            false,
            $contentDiv);
    }

    function showDocuments() {
        CR.SlideOut.closeAllRightSlideOut('DocumentsSlide');
        if (!selectedOrder)
            return;

        let $containerRow = $('#TemplateContentDiv > div > div.row:nth-of-type(2)');

        if (documents === null) {
            getMfgOperationDocuments(getDocsSuccess, null, selectedOrder.MfgOrder, selectedOrder.Product, selectedOrder.Spec);
        }
        else {
            CR.DocSlideOut.open(documents, $containerRow, $containerRow);
        }

        function getDocsSuccess(docSets) {
            documents = [];
            if (Array.isArray(docSets)) {
                docSets.forEach(function (docSet) {
                    documents.push(docSet);
                });
            }
            CR.DocSlideOut.open(documents, $containerRow, $containerRow);
        }
    }

    function getMfgOperationDocuments(successCallback, failCallback, mfgOrderId, productId, specId) {
        var params = {
            mfgOrderId: mfgOrderId ? mfgOrderId : "",
            productId: productId ? productId : "",
            specId: specId ? specId : ""
        };

        $.ajax({
            type: "POST",
            dataType: "json",
            url: './MfgOperationService.svc/web/GetMfgOperationDocs',
            headers: {
                'Accept': 'application/json'
            },
            // Must set content-type this way to avoid jQuery bug with sending JSON containing "??"
            // https://forum.jquery.com/topic/special-characters-issue-find-random-strings-like-jquery20206329934545792639-1415046914457-in-data
            contentType: "application/json;charset=UTF-8",
            async: true,
            data: JSON.stringify(params),
            context: document.body
        })
            .success(successCallback)
            .fail(failCallback);
    }

    function doResize() {
        var height = $('#TemplateContentDiv').height() - _contentHeightAdjust;
        let col = $(SELECTOR.MAIN);
        col.height(height);

        CR.SlideOut.resizeAll();

        let materialHidden = $(SELECTOR.MATERIALS).is(':hidden');

        if (!(materialHidden)) {
            height = height / 2;
            height -= 24;
        } else
            height = $(SELECTOR.DETAILS).height() - 18; //  account for padding
        if (!materialHidden) {
            col = $(SELECTOR.MATERIALS);
            col.height(height);
            let heightAdjust = $(SELECTOR.MATERIALS_HEADER).height() + 18; //  Padding for bottom
            col = $(SELECTOR.MATERIALS_CONTENT);
            col.height(height - heightAdjust);
        }

        if (!mfgOperation.resizeHandler) {
            CR.Grid.fillParentVertical(CTRL_IDS.MATERIALS_GRID_CONTAINER);
            CR.Grid.resizeCols(CTRL_IDS.MATERIALS_GRID, CTRL_IDS.MATERIALS_GRID_CONTAINER, false);
        }
    }


    function showNoData() {
        if ($(SELECTOR.NO_DATA_IMG).length) {
            var emptyStateImg = $(SELECTOR.NO_DATA_IMG)[0],
                ctx = emptyStateImg.getContext('2d'),
                image = new Image(192, 192);
            image.src = 'Themes/Horizon/images/icons/typeComputer48.svg';
            image.onload = function () {
                ctx.drawImage(image,
                    0,
                    0,
                    emptyStateImg.width,
                    emptyStateImg.height);
            };
        }
    }

    // for use during initialization to avoid the screen flashing
    function hideEverything() {
        $(SELECTOR.MATERIALS).hide();
        $(SELECTOR.DETAILS).hide();
        $(SELECTOR.NO_DATA).hide();
    }

    function updateDisplay() {
        documents = null;
        CR.SlideOut.closeAll();
        var show = haveData();

        if (show) {
            $(SELECTOR.CONTENT).show();
            $(SELECTOR.DETAIL_CONTAINER).show();
            $(SELECTOR.DETAILS).show();
            $(SELECTOR.MATERIALS).show();
            $(SELECTOR.NO_DATA).hide();
        }
        else {
            $(SELECTOR.CONTENT).hide();
            $(SELECTOR.DETAIL_CONTAINER).hide();
            $(SELECTOR.DETAILS).hide();
            showNoData();
            $(SELECTOR.NO_DATA).show();
        }

        CR.SideBar.showButton('buttonDocuments', show);
        CR.SideBar.showButton('buttonAttributes', show);
    }

    function setupResizeHandler() {
        _firstResizeDone = false;

        // portal triggers this event when the containing panel is resized.  Use that to resize our table and Active PCB
        $(document).on('panelResized', function (e, bodyHeight) {
            resize();
        });

        // always clear existing handler so not attaching multiple that all execute on each resize.
        $(window).off('resize', onWindowResize).on('resize', onWindowResize).trigger('resize');
    }

    // handler for window resize event
    function onWindowResize() {
        if (_rszTime)
            clearTimeout(_rszTime);
        _rszTime = setTimeout(resize, 250);
    }


    function resize() {

        var height = $('#TemplateContentDiv').height() - _contentHeightAdjust;
        $(SELECTOR.NO_DATA).height(height - 32);
        let col = $(".mfgOperation-tile-col");
        col.height(height);
        col = $(".mfgOperation-data-col");
        col.height(height);
        col = $(".mfgOperation-details-col");
        col.height(height);
        col = $(SELECTOR.DETAIL_CONTAINER);
        col.height(height - 34);
        col = $(SELECTOR.DETAIL_CONTENT);
        let hdr = $(SELECTOR.DETAIL_HEADER);
        col.height(height - 34 - hdr.height());

        col = $(SELECTOR.MAIN);
        if (col.length) {
            col.height(height);
            let headerHeight = $(SELECTOR.MATERIALS_HEADER).height();
            col = $(SELECTOR.MATERIALS_CONTENT);
            col.height(height - headerHeight);
        }

        if (!_firstResizeDone) {
            _firstResizeDone = true;
            updateDisplay();
        }

        doResize();

        if (mfgOperationInterface.resizeHandler)
            mfgOperationInterface.resizeHandler();
    }

    function haveData() {
        return !!(mfgOrderList && mfgOrderList.length > 0);
    }

    var _lastSelectedTile = null;

    // Do any necessary updates to the Mfg Order tiles after they get rendered by core processing.
    function setOrderList(orderList) {

        mfgOrderList = orderList;
        updateDisplay();
        _lastSelectedTile = null;

        // update tiles in timeout to give client processing time to add them to page.
        setTimeout(updateTiles, 200);

        function updateTiles() {
            var $firstTile;

            $(SELECTOR.TILE_LIST).each(function () {
                var $tile = $(this);
                var tileId = $tile.attr('data-custom');
                $tile.click(function () { clickTile(tileId); });
                $tile.css('cursor', 'pointer');

                if (!$firstTile)
                    $firstTile = $tile;
            });

            $('.icon-last').hide();

            if ($firstTile)
                $firstTile.trigger('click');
            else {
                // call any registered handlers
                orderClickHandlers.forEach(function (handler) {
                    handler(null);
                });
            }
        }
    }


    // handler for clicking a tile
    function clickTile(id) {
        updateDisplay();

        // get selected order
        var order;
        mfgOrderList.some(function (ord) {
            if (ord.UniqueId === id) {
                order = ord;
                return true;
            }
        });

        let tile = $('[data-custom="' + id + '"]');
        let prevMfgOrderId = _lastSelectedTile ? _lastSelectedTile.attr("data-custom") : null;
        if (prevMfgOrderId && prevMfgOrderId === id)
            return;

        if (_lastSelectedTile && _lastSelectedTile != tile)
            _lastSelectedTile.removeClass("tile-selected");

        if (tile) {
            tile.addClass("tile-selected");
            _lastSelectedTile = tile;
        }

        rebuildDetails(order);
        selectedOrder = order;

        // call any registered handlers
        orderClickHandlers.forEach(function (handler) {
            handler(order);
        });

        if (order) {
            CR.MfgOperationMaterials.loadMaterialRequirements($(SELECTOR.RESOURCE_NAME).val(), order, isHideNonRequirementChecked());
        }
    }

    // clear and populate details for given order
    function rebuildDetails(order) {
        // clear the details
        var $details = $(SELECTOR.DETAIL_CONTENT_TABLE);
        $details.find('.content-row').remove();

        // update details based on configured properties
        displayDetails.forEach(function (detail) {
            $details.append(
                TEMPLATE.contentRow
                    .replace(/_CONTENT_ROW_NAME_/, labels[detail.label])
                    .replace(/_CONTENT_ROW_VALUE_/, order[detail.value] ? order[detail.value] : ''));
        });
        if (mfgOperationInterface.detailsRebuiltHandler)
            mfgOperationInterface.detailsRebuiltHandler(order);

    }

    function toggleHideNonRequirement() {

        let hideNonRequirement = isHideNonRequirementChecked();
        setCheckBoxChecked(hideNonRequirement);
        CR.MfgOperationMaterials.loadMaterialRequirements($(SELECTOR.RESOURCE_NAME).val(), selectedOrder, hideNonRequirement);
    }

    function setCheckBoxChecked(hideNonRequirement) {
        if (hideNonRequirement) {
            $(SELECTOR.HIDE_NON_REQUIREMENT).prop('checked', true);
            $(SELECTOR.HIDE_NON_REQUIREMENT).attr('checked', 'checked');
        }
        else {
            $(SELECTOR.HIDE_NON_REQUIREMENT).prop('checked', false);
            $(SELECTOR.HIDE_NON_REQUIREMENT).removeAttr('checked');
        }
    }

    function isHideNonRequirementChecked() {
        var $checkboxPanel = $(SELECTOR.HIDE_NON_REQUIREMENT);
        var $checkBox = $checkboxPanel.find('input[type=checkbox]');

        return !!($checkBox.closest('input').prop('checked'));
    }

})();
