// Copyright Siemens 2024

/*
 * CR.PickGrid
 * Shows a grid of objects user may pick from
 */
(function CR_PickGrid() {
    'use strict';

    var api = CR.GetNamespace('CR.PickGrid');

    api.openPopup = openPopup;
    api.toggleAll = toggleAll;
    api.toggleCheckState = toggleCheckState;
    api.setRowStriping = setRowStriping;
    api.sort = sort;

    /**
     * Open a pick grid in a popup
     * 
     * @param {function} [pickFrom] - Objects the user may pick from. Array or object with array properties.
     * @param {function} [configParam] - Object to configure grid columns and other properties
     * @param {function} [confirmCallback] - Optional function to call when user closes popup after selecting object(s).
     * @param {function} [cancelCallback] - Optional function to call when user closes popup by canceling.
    */
    function openPopup(pickFrom, configParam, confirmCallback, cancelCallback) {
        let defaultConfig = {
            multiSelect: true
        };

        let config = $.extend({}, defaultConfig, configParam);

        // if given a single array, wrap it in an object
        let pickFromIsArray = Array.isArray(pickFrom);
        let arraysToPickFrom = pickFromIsArray ? { singleType: pickFrom } : pickFrom;


        // if we are configured to show a single object type, create a single element array for it
        if (config.hasOwnProperty('objectType')) {
            config.objectType.objsToPickPropName = 'singleType';
            config.objectTypes = [config.objectType];
        }

        // function to return a locally unique integer ID
        let getUniqueId = (function () {
            // closure to hide current ID from outside world
            let uId = 0;
            return function () {
                return uId++;
            };
        })();

        let tabsMarkup = '';
        let tabPagesMarkup = '';
        let isFirstTab = true;

        // build markup for tabs and contents of tab pages (grids)
        config.objectTypes.forEach(function (objType) {
            // generate IDs for each object type
            objType.objTypeId = 'obj-type-' + getUniqueId();

            // get corresponding array of objects to pick from
            let objsToPickFrom = arraysToPickFrom[objType.objsToPickPropName];

            // tab markup
            tabsMarkup += getTabMarkup(objType, isFirstTab);

            // tab page markup
            tabPagesMarkup += getTabPageMarkup(objType, objsToPickFrom);

            isFirstTab = false;
        });

        let contentMarkup =
            `<div id='cr-pick-grid'>
                <div id='cr-pick-grid-header'>
                    <div class='cr-search'>
                        <input type='text' id='pick-grid-filter' autocomplete='off'/>
                        <span></span>
                    </div>
                </div>

                <div class='tab-container ui-tabs ui-corner-all ui-widget ui-widget-content wrap-mode' id='cr-pick-grid-tab-container'>
                    <div class='cs-nav-tabs'>
                        <div class='cs-nav-wrapper'>
                            <ul class='ui-tabs-nav ui-helper-clearfix ui-corner-all ui-helper-reset ui-widget-header' role='tablist'>
                                ${tabsMarkup}
                            </ul>
                        </div>
                    </div>
                    <div class='ui-tabs-streak'></div>
                    <div id='cr-pick-grid-tab-pages' class='cs-tab-pages'>
                        ${tabPagesMarkup}
                    </div>
                </div>

                <div id='divButtons' class='cr-button-footer'>
                    <input type='button' id='button-cancel' value='Close' labelid='CancelButton' class='cs-button-secondary' />
                    <input type='button' id='button-confirm' value='Confirm' labelid='PS_ACTION_OK' class='cs-button' />
                </div>
            </div>`;

        var $content = $(contentMarkup);

        // Add handler for select all
        $content.find('th span.select-all-check-span > input').on('change', toggleAll);
        // Handlers for full row select
        $content.find(".cr-pick-table > tbody tr").click(rowClicked);
        // Handler for filtering
        $content.find('#pick-grid-filter').on('input', filterChanged);
        $content.find('#button-confirm').on('click', confirmClicked);
        $content.find('#button-cancel').on('click', cancelClicked);
        // Header cell row click hander (for sorting)
        $content.find('th[cr-prop-name]').on('click', function () { sort($(this), $content, rowClicked); });

        $content.find('li > a').on('click', onClickTab);

        // If we have just a single type of object, no need to show tabs
        if (config.objectTypes.length === 1) {
            $content.find('.cs-nav-tabs').hide();
        }

        $content.find('tbody tr:even').addClass('even');

        // localize content now.  Load additional labels below
        CR.Page.localizePage(localized, $content);

        //
        function localized() {
            // hide any tab after the first one
            $content.find('div#cr-pick-grid-tab-pages > div:not(:first)').hide();

            // insert modal overlay, if not already present
            var $overlay = $('#cr-modal-overlay');
            if ($overlay.length === 0) {
                $(document.body).append('<div class="divModal" id="cr-modal-overlay"></div>');
                $overlay = $('#cr-modal-overlay');
            }

            $overlay.show();
            var defaultWidth = $(window).outerWidth() * 0.9;   // default to 90% height/width
            var defaultHeight = $(window).outerHeight() * 0.9;

            var defaultParams = {
                resizable: true,
                minheight: 360,
                minwidth: 300,
                height: defaultHeight,
                width: defaultWidth,
                modal: true,
                closeOnEscape: true,
                title: 'Select',
                open: function () { $('.ui-dialog').css('z-index', 9999); },
                close: function (ev, ui) {
                    $content.dialog("destroy");
                    $overlay.hide();
                },
                classes: {
                    'ui-dialog-titlebar': '' // tell jquery to remove default styling associated with title bar (corner radius)
                }
            };

            // overlay any given params onto the default params
            var resolvedParams = $.extend({}, defaultParams, config);

            $content
                .dialog(resolvedParams)
                .keypress(function (e) {
                    if (e.keyCode === $.ui.keyCode.ENTER)
                        $content.dialog("close");
                });
            $content.dialog("open");

            setTimeout(function () {
                let firstObjType = config.objectTypes[0];
                CR.Grid.makeScrollable(`cr-pick-table-${firstObjType.objTypeId}`, 'cr-pick-grid-tab-pages');
                $(`cr-grid-picker-tab-page-${firstObjType.objTypeId}`).attr('cr-scrollable', 'true');

                // load labels that can't be localized with "labelid"
                var labels = [{ Name: 'LblMenuSearch' }];
                if (config.titleLabelName) {
                    labels.push({ Name: config.titleLabelName });
                }

                __page.getLabels(labels, function (result) {
                    if ($.isArray(result)) {
                        // success
                        result.forEach(l => {
                            // set known labels to the appropriate DOM elements
                            if (l.Name === 'LblMenuSearch') {
                                $content.find('input#pick-grid-filter').attr('placeholder', l.Value);
                            } else if (l.Name === config.titleLabelName) {
                                $content.dialog('option', 'title', l.Value);
                            } else {
                                console.warn(`Unknown label: ${l.Name}`);
                            }
                        });
                    } else {
                        // fail
                        console.error(result.Error);
                    }
                });

            }, 0);
        }

        //
        function confirmClicked() {
            if (!confirmCallback) {
                $content.dialog("close");
                return;
            }

            // if we were given an array, give one back
            let pickedObjs = pickFromIsArray ? [] : {};

            // selected <tr>s
            $content.find('div.cs-tab-pages div.ui-tabs-panel').each(function () {
                let $panel = $(this);
                let objTypeId = $panel.attr('cr-obj-type-id');
                let objType = config.objectTypes.find(ot => ot.objTypeId === objTypeId);
                let objsToPickFrom = arraysToPickFrom[objType.objsToPickPropName];

                if (!pickFromIsArray) {
                    pickedObjs[objType.objsToPickPropName] = [];
                }

                $panel.find("table.cr-pick-table tr span.cr-checkbox[checked='checked']").each(function () {
                    let $tr = $(this).closest('tr');

                    let pickedObj = objsToPickFrom.find(objToPick => objToPick[objType.idPropName] === $tr.attr('cr-obj-type-id'));

                    if (pickedObj) {
                        if (pickFromIsArray) {
                            pickedObjs.push(pickedObj);
                        } else {
                            pickedObjs[objType.objsToPickPropName].push(pickedObj);
                        }
                    }
                });
            });

            confirmCallback(pickedObjs);
            $content.dialog("close");
        }

        // 
        function cancelClicked() {
            closePopup();

            if (cancelCallback) {
                cancelCallback();
            }
        }

        //
        function filterChanged(e) {
            let filterValue = e.target.value.toUpperCase();

            $('.cr-pick-table > tbody > tr')
                .removeClass('cr-collapse')     // remove class that makes rows collapse
                .each(function () {
                    if (filterValue) {
                        // completely collapse any rows whose filtering value does not begin with the filter value
                        let objFilterValue = $(this).attr('cr-filter-value');
                        if (!objFilterValue.includes(filterValue)) {
                            $(this).addClass('cr-collapse');
                        }
                    }

                });

            setRowStriping($content);
        }

        // return markup for a tab (just the tab header, not the content)
        function getTabMarkup(objType, isActive) {
            let activeClass = isActive ? 'ui-state-active ui-tabs-active' : '';
            let labelMarkup = objType.displayLabelName ? `labelid='${objType.displayLabelName}'` : '';

            let tabMarkup =
                `<li id='cr-grid-picker-tab-${objType.objTypeId}' class='ui-state-default ui-corner-top ui-tabs-tab ui-tab ${activeClass}' role='tab'>
                 <a href='#' cr-obj-type-id='${objType.objTypeId}' class='ui-tabs-anchor' ${labelMarkup}>${objType.displayText}</a>
             </li>`;

            return tabMarkup;
        }

        // 
        function getTabPageMarkup(objType, objectsToPick) {
            let headerCellsMarkup = getHeaderCellsMarkup(objType);
            let rowsMarkup = getRowsMarkup(objType, objectsToPick);

            let tabPageMarkup =
                `<div id='cr-grid-picker-tab-page-${objType.objTypeId}' cr-obj-type-id='${objType.objTypeId}' class='ui-tabs-panel ui-widget-content ui-corner-bottom'>
                 <table id='cr-pick-table-${objType.objTypeId}' class='cr-pick-table ui-jqgrid ui-jqgrid-hdiv'>
                     <thead>
                         <tr class='ui-jqgrid-labels'>
                             ${headerCellsMarkup}
                         </tr>                                                                                                          
                     </thead>                                                                                                           
                     <tbody>
                         ${rowsMarkup}
                     </tbody>
                 </table>
             </div>`;

            return tabPageMarkup;
        }

        //
        function getHeaderCellsMarkup(objType) {
            let hiddenClass = config.multiSelect ? '' : 'cr-hidden';

            // check box column
            let headersCellsMarkup =
                `<th class='cr-col-check ${hiddenClass}'>                                                    
                    <span class='select-all-check-span cs-checkbox cr-checkbox' >                                     
                        <input type='checkbox' id='select-all-${objType.objTypeId}' itemId='' />
                        <label for='select-all-${objType.objTypeId}'></label>
                    </span>
                </th >`;

            // configured columns
            objType.colDefs.forEach(colDef => {
                let labelMarkup = colDef.headerLabelName ? `labelid='${colDef.headerLabelName}'` : '';
                headersCellsMarkup += `<th ${labelMarkup} cr-prop-name='${colDef.propName}' > <span>${colDef.headerText}</span></th >`;
            });

            return headersCellsMarkup;
        }

        //
        function getRowsMarkup(objType, objectsToPick) {
            let rowsMarkup = '';
            let hiddenMarkup = config.multiSelect ? '' : `class='cr-hidden'`;

            objectsToPick.forEach(objToPick => {
                // check box column
                let cellsMarkup =
                    `<td ${hiddenMarkup}>
                    <span id='container-check-${objToPick[objType.idPropName]}-span' class='cs-checkbox cr-checkbox' >
                        <input type='checkbox' id='container-check-${objToPick[objType.idPropName]}' itemId='${objToPick[objType.idPropName]}' />
                        <label for='container-check-${objToPick[objType.idPropName]}'></label>
                    </span>
                </td>`;

                // configured columns
                objType.colDefs.forEach(colDef => {
                    cellsMarkup += `<td>${objToPick[colDef.propName] ?? ''}</td >`;
                });

                // put cells into row and add to rows
                rowsMarkup += `<tr cr-obj-type-id='${objToPick[objType.idPropName]}' cr-filter-value='${objToPick[objType.filterPropName].toUpperCase()}' >${cellsMarkup}</tr >`;
            });

            return rowsMarkup;
        }

        //
        function onClickTab(e) {
            e.preventDefault();
            let $tabAnchor = $(this);
            let clickedObjTypeId = $tabAnchor.attr('cr-obj-type-id');

            // update classes for active tab
            $content.find('ul.ui-tabs-nav li').removeClass('ui-state-active ui-tabs-active');
            $tabAnchor.closest('li').addClass('ui-state-active ui-tabs-active');

            let $firstTimePanel = null;

            // show/hide tab pages
            $content.find('div.cs-tab-pages div.ui-tabs-panel').each(function () {
                let $tabPanel = $(this);
                if ($tabPanel.attr('cr-obj-type-id') === clickedObjTypeId) {
                    $tabPanel.show();

                    // first time we show it, we have to have it make scrollable
                    if ($tabPanel.attr('cr-scrollable') !== 'true') {
                        $firstTimePanel = $tabPanel;
                    }
                } else {
                    $tabPanel.hide();
                }
            });

            // after other tab panels are hidden we can now make this one scrollable
            if ($firstTimePanel) {
                $firstTimePanel.attr('cr-scrollable', 'true');
                CR.Grid.makeScrollable($firstTimePanel.find('table').attr('id'), 'cr-pick-grid-tab-pages');
            }
        }

        //
        function rowClicked(e) {
            // if clicking in the checkbox itself, get two events. ignore the first so only pcocess once. 

            var $row = $(this);
            var $checkBox = $row.find('input[type=checkbox]');
            toggleCheckState($checkBox);

            if (!config.multiSelect) {
                confirmClicked();
            }

            return false;
        }

        /**
         * Closes this pick grid popup
         * 
         */
        function closePopup() {
            $content.dialog("close");
        }

        // Object for client to interact with this popup
        var popupReturn = {
            closePopup: closePopup
        };

        return popupReturn;
    }

    // Toggle all row selector check boxes on or off
    function toggleAll(event) {
        event.preventDefault();
        let $clickedSelectAllInput = $(this);
        let checked = $clickedSelectAllInput.is(':checked');

        // change state of check box in header
        setCheckBoxChecked($clickedSelectAllInput.closest('span'), checked);

        // set check state of all body rows
        var $nonEmptyRows = $clickedSelectAllInput.closest(".cr-pick-table").find(" > tbody > tr:not([id^='#empty#'])");
        $nonEmptyRows.each(function () {
            setCheckBoxChecked($(this).find('span.cr-checkbox'), checked);
        });
    }

    // 
    function setCheckBoxChecked($spanContainingCB, checked) {
        // Classic requires the nested <input> to also be marked checked
        var $checkElems = $spanContainingCB.add($spanContainingCB.find('> input'));

        // fixed View Mode radio button option, replace attr with prop
        if (checked) {
            $checkElems.prop('checked', true);
            $checkElems.attr('checked', 'checked');
        } else {
            $checkElems.prop('checked', false);
            $checkElems.removeAttr('checked');
        }
    }

    //
    function toggleCheckState($checkBox) {
        let $containingSpan = $checkBox.closest('span');
        var newCheckState = !($containingSpan.attr('checked'));  // 'checked' attribute is kept on the <span> parent of the checkbox input
        setCheckBoxChecked($containingSpan, newCheckState);
        return newCheckState;
    }

    // remove styling for row stripes and make sure only even rows are striped
    function setRowStriping($tableContainer) {
        $tableContainer.find('tbody tr').removeClass('even');
        $tableContainer.find('tbody').each(function () {
            let $tbody = $(this);
            // do NOT consider collapsed rows for striping
            $tbody.find('tr:not(.cr-collapse):even').addClass('even');
        });
    }

    // 
    function sort($clickedHeader, $tableContainer, rowClicked) {
        //let $clickedHeader = $(this);

        let $spanInHeader = $clickedHeader.find('span');
        let alreadySortedAsc = $spanInHeader.hasClass('sort-asc');

        // remove any sort arrows from all column headers
        $clickedHeader.closest('thead').find('th span').removeClass('sort-asc').removeClass('sort-desc');

        if (alreadySortedAsc) {
            $spanInHeader.addClass('sort-desc');
        } else {
            $spanInHeader.addClass('sort-asc');
        }

        let $tbody = $clickedHeader.closest('table').find('tbody');
        let $trs = $tbody.find('tr');
        let headerIndex = $clickedHeader.index();

        // remove click handlers and rows from body
        $tbody.find('th[cr-prop-name]').off('click');
        $tbody.find("tr").off('click');
        $tbody.empty();

        // sort
        $trs.sort((l, r) => {
            let left = $(l).children().eq(headerIndex).html();
            let right = $(r).children().eq(headerIndex).html();
            // reverse sort direction if we doing descending
            let ascDesc = alreadySortedAsc ? -1 : 1;
            return left.localeCompare(right, 'en', { sensitivity: 'base' }) * ascDesc;
        });

        // readd rows
        $tbody.append($trs);
        $tbody.find('th[cr-prop-name]').on('click', function () { sort($(this), $tableContainer, rowClicked); });
        $tbody.find("tr").click(rowClicked);

        // fix stripes
        CR.PickGrid.setRowStriping($tableContainer);
    }

})();