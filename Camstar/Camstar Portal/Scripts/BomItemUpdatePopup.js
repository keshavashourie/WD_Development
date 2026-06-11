// Copyright Siemens 2025

/*
 */
(function CR_BomItemUpdate() {
    'use strict';

    var SELECTORS = {
        NEW_ISSUE_CONTROL_EDIT: '#ctl00_WebPartManager_NewValuesWP_IssueControl_Edit',
        NEW_ISSUE_CONTROL_VALUE: '#ctl00_WebPartManager_NewValuesWP_IssueControl_Value',        // string representation of the enum integer
        NEW_PRODUCT_NAME: '#ctl00_WebPartManager_NewValuesWP_Product_Edit',
        NEW_PRODUCT_REVISION: '#ctl00_WebPartManager_NewValuesWP_Product_Rev',
        NEW_SPEC: '#ctl00_WebPartManager_NewValuesWP_Spec',
        NEW_SPEC_NAME: '#ctl00_WebPartManager_NewValuesWP_Spec_Edit',
        NEW_SPEC_REVISION: '#ctl00_WebPartManager_NewValuesWP_Spec_Rev',
        NEW_ROUTE_STEP: '#ctl00_WebPartManager_NewValuesWP_RouteStep',
        NEW_ROUTE_STEP_NAME: '#ctl00_WebPartManager_NewValuesWP_RouteStep_Edit',
        NEW_QUANTITY: '#ctl00_WebPartManager_NewValuesWP_Quantity_ctl00',
        BOM_ITEMS_JSON: 'input[name$="$BomItemsJson$ctl00"]',
        UPDATED_BOM_ITEMS_JSON: '#ctl00_WebPartManager_NewValuesWP_UpdatedBomItemsJson_ctl00',
        //ERP_ROUTE_NAME: '#ctl00_WebPartManager_NewValuesWP_ERPRoute_Edit',
        UPDATE: '#ctl00_WebPartManager_ButtonsBar_CustomActionUpdate',
        APPLY: '#ctl00_WebPartManager_NewValuesWP_Apply'
    };

    let COLS = {
        NAME: 0,
        SELECTED: 1,
        REFDES: 2,
        ISSUE_CONTROL: 3,
        PRODUCT: 4,
        SPEC: 5,
        ROUTE_STEP: 6,
        QUANTITY: 7
    }

    var api = CR.GetNamespace('CR.BomItemUpdatePopup');

    api.initialize = initialize;

    //
    function initialize(showSpec, showRouteStep) {

        let bomItemsText = $(SELECTORS.BOM_ITEMS_JSON).val();
        let bomItems = JSON.parse(bomItemsText); 

        if (!showSpec) {
            $(SELECTORS.NEW_SPEC).addClass('displayNone');
        }

        if (!showRouteStep) {
            $(SELECTORS.NEW_ROUTE_STEP).addClass('displayNone');
        }

        createGrid(bomItems, showSpec, showRouteStep);
    }

    /**
     * Insert markup for the grid of BOM items
     * 
    */
    function createGrid(bomItems, showSpec, showRouteStep) {

        // tab page markup
        let tableMarkup = getTableMarkup(bomItems, showSpec, showRouteStep);

        let contentMarkup =
            `
            <div id='cr-pick-grid'>
                <div class='cr-search'>
                    <input type='text' id='pick-grid-filter' autocomplete='off'/>
                    <span></span>
                </div>

                ${tableMarkup}
            </div>
            `;

        var $content = $(contentMarkup);

        // Add handler for select all
        $content.find('th span.select-all-check-span > input').on('change', CR.PickGrid.toggleAll);
        // Handlers for full row select
        $content.find(".cr-pick-table > tbody tr").click(rowClicked);
        // Handler for filtering
        $content.find('#pick-grid-filter').on('input', filterChanged);
        // Header cell row click hander (for sorting)
        $content.find('th[cr-prop-name]').on('click', function () { CR.PickGrid.sort($(this), $content, rowClicked); });

        $content.find('tbody tr:even').addClass('even');

        $('#bom-item-update').append($content);
        $(SELECTORS.APPLY).on('click', applyClicked);
        $(SELECTORS.UPDATE).on('click', updateClicked);

        // grid data to hidden field - popup will then close
        function updateClicked() {
            let updateItems = [];

            // construct objects from grid data
            $('table.cr-pick-table tbody tr').each(function () {
                let $tr = $(this);
                let $tds = $(this).find('td');

                let updateItem = {
                    Name: $tds.eq(COLS.NAME).text(),
                    RefDes: $tds.eq(COLS.REFDES).text(),
                    IssueControl: $tds.eq(COLS.ISSUE_CONTROL).attr('issue-control-int'),
                    ProductNameRev: $tds.eq(COLS.PRODUCT).text(),
                    SpecNameRev: $tds.eq(COLS.SPEC).text(),
                    RouteStepName: $tds.eq(COLS.ROUTE_STEP).text(),
                    Quantity: $tds.eq(COLS.QUANTITY).text()
                };

                updateItems.push(updateItem);
            });

            // serialize to hidden field to hand back
            $(SELECTORS.UPDATED_BOM_ITEMS_JSON).val(JSON.stringify(updateItems));
        }

        // apply values in "new" value controls to all selected rows
        function applyClicked(e) {
            CR.Event.stopEvent(e);

            let newIssueControl = $(SELECTORS.NEW_ISSUE_CONTROL_EDIT).val();
            let newIssueControlInt = $(SELECTORS.NEW_ISSUE_CONTROL_VALUE).val();
            let newProduct = getNameRev(SELECTORS.NEW_PRODUCT_NAME, SELECTORS.NEW_PRODUCT_REVISION);
            let newSpec = getNameRev(SELECTORS.NEW_SPEC_NAME, SELECTORS.NEW_SPEC_REVISION);
            let newRouteStep = $(SELECTORS.NEW_ROUTE_STEP_NAME).val();
            let newQuantity = $(SELECTORS.NEW_QUANTITY).val();

            $('.cr-pick-table > tbody > tr')
                .each(function () {
                    let $cb = $(this).find('td > span.cs-checkbox');

                    // is this row selected?
                    if ($cb.attr("checked") === "checked") {

                        // update values
                        let $tds = $(this).find('td');

                        if (newIssueControl) {
                            $tds.eq(COLS.ISSUE_CONTROL).text(newIssueControl).attr('issue-control-int', newIssueControlInt);
                        }

                        if (newProduct) {
                            $tds.eq(COLS.PRODUCT).text(newProduct);
                        }

                        if (newSpec) {
                            $tds.eq(COLS.SPEC).text(newSpec);
                        }

                        if (newRouteStep) {
                            $tds.eq(COLS.ROUTE_STEP).text(newRouteStep);
                        }

                        if (newQuantity) {
                            $tds.eq(COLS.QUANTITY).text(newQuantity);
                        }
                    }
                });

            // get colon-delimited name:rev string
            function getNameRev(nameSelector, revSelector) {
                let name = $(nameSelector).val();
                let rev = $(revSelector).val();

                // remove empty value and colon-delimit
                return [name, rev].filter(p => p).join(':');
            }
        }

        //
        function filterChanged(e) {
            let filterValue = e.target.value.toUpperCase();

            $('.cr-pick-table > tbody > tr')
                .removeClass('cr-collapse')     // remove class that makes rows collapse
                .each(function () {
                    let $tds = $(this).find('td');
                    if (filterValue) {
                        // completely collapse any rows whose filtering value does not include the filter value
                        if (!$tds.eq(COLS.REFDES).text().toUpperCase().includes(filterValue)
                            && !$tds.eq(COLS.ISSUE_CONTROL).text().toUpperCase().includes(filterValue)
                            && !$tds.eq(COLS.PRODUCT).text().toUpperCase().includes(filterValue)
                            && !$tds.eq(COLS.SPEC).text().toUpperCase().includes(filterValue)
                            && !$tds.eq(COLS.ROUTE_STEP).text().toUpperCase().includes(filterValue)
                            && !$tds.eq(COLS.QUANTITY).text().toUpperCase().includes(filterValue)) {
                            $(this).addClass('cr-collapse');
                        }
                    }

                });

            CR.PickGrid.setRowStriping($content);
        }

        // 
        function getTableMarkup(bomItems, showSpec, showRouteStep) {
            let headerCellsMarkup = getHeaderCellsMarkup(showSpec, showRouteStep);
            let rowsMarkup = getRowsMarkup(bomItems, showSpec, showRouteStep);

            let tableMarkup =
                `<table id='cr-pick-table' class='cr-pick-table ui-jqgrid ui-jqgrid-hdiv'>
                     <thead>
                         <tr class='ui-jqgrid-labels'>
                             ${headerCellsMarkup}
                         </tr>                                                                                                          
                     </thead>                                                                                                           
                     <tbody>
                         ${rowsMarkup}
                     </tbody>
                 </table>`;

            return tableMarkup;
        }

        //
        function getHeaderCellsMarkup(showSpec, showRouteStep) {
            // check box column
            let headersCellsMarkup =
                `
                <th class='cr-hidden'></th>
                <th class='cr-col-check'>
                    <span class='select-all-check-span cs-checkbox cr-checkbox' >
                        <input type='checkbox' id='select-all' itemId='' />
                        <label for='select-all'></label>
                    </span>
                </th>
                <th labelid='' cr-prop-name=''> <span>RefDes</span> </th>
                <th labelid='' cr-prop-name=''> <span>Issue Control</span> </th>
                <th labelid='' cr-prop-name=''> <span>Product</span> </th>
                <th labelid='' cr-prop-name='' class='${showSpec ? '' : 'displayNone'}'> <span>Spec</span> </th>
                <th labelid='' cr-prop-name='' class='${showRouteStep ? '' : 'displayNone'}'> <span>Route Step</span> </th>
                <th labelid='' cr-prop-name=''> <span>Quantity</span> </th>
                `;

            return headersCellsMarkup;
        }

        //
        function getRowsMarkup(bomItems, showSpec, showRouteStep) {
            let rowsMarkup = '';

            bomItems.forEach(bomItem => {

                let cellsMarkup =
                    // check box column
                    `
                    <td class='cr-hidden'>${bomItem.Name ?? ''}</td>
                    <td>
                        <span class='cs-checkbox cr-checkbox' >
                            <input type='checkbox' id='container-check-${bomItem.Name}' itemId='${bomItem.Name}' />
                            <label for='container-check-${bomItem.Name}'></label>
                        </span>
                    </td>
                    <td>${bomItem.ReferenceDesignator ?? ''}</td>
                    <td>${bomItem.IssueControl ?? ''}</td>
                    <td>${bomItem.Product ?? ''}</td>
                    <td class='${showSpec ? '' : 'displayNone'}'>${bomItem.Spec ?? ''}</td>
                    <td class='${showRouteStep ? '' : 'displayNone'}'>${bomItem.RouteStep ?? ''}</td>
                    <td>${bomItem.QtyRequired ?? ''}</td>`;

                // put cells into row and add to rows
                rowsMarkup += `<tr cr-obj-type-id='${bomItem.Name}'>${cellsMarkup}</tr >`;
            });

            return rowsMarkup;
        }

        //
        function rowClicked(e) {
            // if clicking in the checkbox itself, get two events. ignore the first so only process once. 

            var $row = $(this);
            var $checkBox = $row.find('input[type=checkbox]');
            CR.PickGrid.toggleCheckState($checkBox);

            return false;
        }
    }
})();