// Copyright 2020 Siemens AG

/*global
 import
 */

/**
 * A minimal service used by the MOM ACB Component. For more information on how to use this component, see the [MOM ACB Component](https://gitlab.industrysoftware.automation.siemens.com/mom/mom-ui/wikis/mom-acb-component) page of the MOM UI Kit Wiki.
 * @module "js/mom.acb.component.service"
 */
import app from 'app';
import _ from 'lodash';
import _appCtxSvc from 'js/appCtxService';
import _vMSvc from 'js/viewModelService';
import _eventBus from 'js/eventBus';
import _awColFilterSrv from 'js/awColumnFilterService';
import _utilsSvc from 'js/mom.utils.service.js';
import _viewModeService from 'js/viewMode.service';
import _uwPropertySvc from 'js/uwPropertyService';
import _configSvc from 'js/configurationService';
import _popupSvc from 'js/popupService';
import $ from 'jquery';

'use strict';
const exports = {};
var momAcbConfiguration = _appCtxSvc.getCtx( 'momAcbConfiguration' );
var isFirstTime = ( function() {
    var length = momAcbConfiguration.tableProperties.columns.selected.length;
    return function( reset ) {
        if( reset ) {
            var currentMode = _viewModeService.getViewMode();
            var isTable = currentMode === 'TableView' || currentMode === 'TableTreeView' || currentMode === 'SummaryView' ? true : false;
            length = isTable ? momAcbConfiguration.tableProperties.columns.selected.length + 1 : momAcbConfiguration.listProperties.propDefinitions.propNames.length + 1;
        }
        if( length > 0 ) {
            length -= 1;
            return true;
        }
        return false;
    };
} )();
exports.updateViewFunction = function() {

    _eventBus.publish( momAcbConfiguration.tableProperties.id + ".plTable.reload" );
};
exports.updateListViewFunction = function() {
    _eventBus.publish( momAcbConfiguration.listProperties.id + ".contentLoaded" );
};
exports.createLeftSideData = function( radioViewList ) {

    var columns;
    var result = [];
    if( momAcbConfiguration !== undefined ) {
        radioViewList.dbValue = momAcbConfiguration.isDefaultViewTable;
        columns = momAcbConfiguration.tableProperties.columns.available;
        result = [];
        columns.forEach( element => {
            var item = _uwPropertySvc.createViewModelProperty( "revisionNumberValues", "", "STRING", [], element.name );
            item.uiValue = element.uiValue;
            result.push( item );
        } );
        return {
            leftSideData: result,
            radioViewList: radioViewList
        };
    }
};
exports.createListDataFromConfig = function() {

    var result = [];
    if( momAcbConfiguration !== undefined ) {
        momAcbConfiguration.listProperties.propDefinitions.props.forEach( element => {
            var acbListBox = _uwPropertySvc.createViewModelProperty( "acbListBox", "", "STRING", [], element.acbListBox.dbValue );
            _uwPropertySvc.setHasLov( acbListBox, true );
            _uwPropertySvc.setIsSelectOnly( acbListBox, true );
            _uwPropertySvc.setIsEnabled( acbListBox, true );
            _uwPropertySvc.setEditable( acbListBox, true );
            _uwPropertySvc.setIsEditable( acbListBox, true );
            acbListBox.propApi = {};
            acbListBox.displayName = element.acbListBox.displayName;
            acbListBox.propertyDisplayName = element.acbListBox.displayName;
            acbListBox.dispValue = element.acbListBox.dispValue;

            // Arrange options in saved order
            let index = element.acbListBoxValues.dbValue.findIndex( x => x.dispValue === element.acbListBox.uiValue );
            element.acbListBoxValues.dbValue.unshift( element.acbListBoxValues.dbValue.splice( index, 1 )[ 0 ] );

            var resultElem = {
                acbListBox: acbListBox,
                acbListBoxValues: element.acbListBoxValues
            };

            result.push( resultElem );
        } );
    }
    return {
        listDataObject: result
    };
};
exports.createRightSideData = function( radioViewList ) {

    var columns;
    var result = [];
    if( momAcbConfiguration !== undefined ) {
        columns = momAcbConfiguration.tableProperties.columns.selected;
        result = [];
        columns.forEach( element => {
            var item = _uwPropertySvc.createViewModelProperty( "revisionNumberValues", "", "STRING", [], element.name );
            item.uiValue = element.uiValue;
            result.push( item );
        } );
        return {
            rightSideData: result,
            radioViewList: radioViewList
        };
    }
};
exports.openSimpleFilter = function() {
    var toggle = _appCtxSvc.getCtx( 'simpleFilterIsSelected' );
    _appCtxSvc.updateCtx( 'simpleFilterIsSelected', !toggle );
    isFirstTime( true );
};
exports.openStandardSortMenu = function() {
    var currentMode = _viewModeService.getViewMode();
    var commandsVMPromise = _utilsSvc.getCfg( 'commandsViewModel' );
    var isTable = currentMode === 'TableView' || currentMode === 'TableTreeView' || currentMode === 'SummaryView' ? true : false;

    if( isTable ) {
        commandsVMPromise.then( function( commandVM ) {
            var control = momAcbConfiguration.tableProperties.id;
            cleanCommandVM( commandVM, "listSortOption" );
            momAcbConfiguration.tableProperties.columns.selected.forEach( column => {
                if( column.name !== 'icon' ) {
                    // Add command in commands
                    commandVM.commands[ column.name ] = {
                        title: column.uiValue,
                        identifier: "tableSortOption"
                    };
                    // Add command Handler
                    commandVM.commandHandlers[ column.name + "Handler" ] = {
                        id: column.name,
                        action: column.name + "Action",
                        activeWhen: true,
                        visibleWhen: true,
                        selectedWhen: "ctx.momACBCurrentSort === " + column.name,
                        identifier: "tableSortOption"

                    };
                    // Add command Placements
                    commandVM.commandPlacements[ column.name + "Placement" ] = {
                        id: column.name,
                        uiAnchor: "momACBDefaultCommands",
                        priority: 30,
                        parentGroupId: "cmdStandardBarSort",
                        showGroupSelected: true,
                        identifier: "tableSortOption"
                    };
                    // Add command Actions
                    commandVM.actions[ column.name + "Action" ] = {
                        actionType: "JSFunction",
                        method: "sortAction",
                        identifier: "tableSortOption",
                        inputData: {
                            sortInput: {
                                vm: control,
                                columnName: column.name,
                                isAscending: true

                            }
                        },
                        deps: "js/mom.acb.component.service"
                    };
                }
            } );
            _utilsSvc.updateCommands( commandVM );
        } );
    } else {
        commandsVMPromise.then( function( commandVM ) {
            var control = momAcbConfiguration.listProperties.id;
            var props = momAcbConfiguration.listProperties.propDefinitions.propNames;
            // update order
            let propsOrder = momAcbConfiguration.listProperties.propDefinitions.props;
            let listOrder = [];
            propsOrder.forEach( element => {
                listOrder.push( element.acbListBox.dbValue );
            } );
            var finalProps = [];
            listOrder.forEach( orderElem => {
                let pushItem = props.find( x => x.internalName === orderElem );
                if( pushItem !== undefined ) {
                    finalProps.push( pushItem );
                }
            } );
            cleanCommandVM( commandVM, "tableSortOption" );
            finalProps.forEach( prop => {
                // Add command in commands
                commandVM.commands[ prop.internalName ] = {
                    title: prop.displayName,
                    identifier: "listSortOption"
                };
                // Add command Handler
                commandVM.commandHandlers[ prop.internalName + "Handler" ] = {
                    id: prop.internalName,
                    action: prop.internalName + "Action",
                    activeWhen: true,
                    visibleWhen: true,
                    selectedWhen: "ctx.momACBCurrentSort === " + prop.internalName,
                    identifier: "listSortOption"

                };
                // Add command Placements
                commandVM.commandPlacements[ prop.internalName + "Placement" ] = {
                    id: prop.internalName,
                    uiAnchor: "momACBDefaultCommands",
                    priority: 30,
                    parentGroupId: "cmdStandardBarSort",
                    showGroupSelected: true,
                    identifier: "listSortOption"
                };
                // Add command Actions
                commandVM.actions[ prop.internalName + "Action" ] = {
                    actionType: "JSFunction",
                    method: "sortListAction",
                    inputData: {
                        sortInput: {
                            vm: control,
                            columnName: prop.internalName,
                            isAscending: true

                        }
                    },
                    identifier: "listSortOption",
                    deps: "js/mom.acb.component.service"
                };
            } );
            _utilsSvc.updateCommands( commandVM );
        } );
    }
};
exports.openStandardGroupMenu = function() {
    var commandsVMPromise = _utilsSvc.getCfg( 'commandsViewModel' );
    commandsVMPromise.then( function( commandVM ) {
        momAcbConfiguration.tableProperties.groupProperties.groupOptions.forEach( column => {
            if( column.name !== 'icon' ) {
                // Add command in commands
                commandVM.commands[ "groupByOpt" + column.name ] = {
                    title: column.uiValue
                };
                // Add command Handler
                commandVM.commandHandlers[ "groupByOpt" + column.name + "Handler" ] = {
                    id: "groupByOpt" + column.name,
                    action: "groupByOpt" + column.name + "Action",
                    activeWhen: true,
                    visibleWhen: true,
                    selectedWhen: "ctx.momACBCurrentGroup === " + "groupByOpt" + column.name

                };
                // Add command Placements
                commandVM.commandPlacements[ "groupByOpt" + column.name + "Placement" ] = {
                    id: "groupByOpt" + column.name,
                    uiAnchor: "momACBDefaultCommands",
                    priority: 30,
                    parentGroupId: "cmdGroupByTable",
                    showGroupSelected: true
                };
                // Add command Actions
                commandVM.actions[ "groupByOpt" + column.name + "Action" ] = {
                    actionType: "JSFunction",
                    method: "openGroupByMenu",
                    inputData: {
                        id: "groupByOpt" + column.name
                    },
                    deps: "js/mom.acb.component.service"
                };
            }
        } );
    } );
};
exports.openGroupByMenu = function( id ) {
    var viewModeObj = _appCtxSvc.getCtx( 'ViewModeContext' );
    let viewMode = {
        "supportedViewModes": {
            "SummaryView": {},
            "TableSummaryView": {},
            "ListView": {},
            "TableView": {}
        },
        "ViewModeContext": "TableTreeView"
    };
    var commandsVMPromise = _utilsSvc.getCfg( 'commandsViewModel' );
    commandsVMPromise.then( function( commandVM ) {
        viewMode.ViewModeContext = viewModeObj.ViewModeContext === "ListView" ? "ListGroupView" : "TableTreeView";
        for( let key in commandVM.commands ) {
            if( commandVM.commands.hasOwnProperty( key ) ) {
                if( key.includes( "groupByOpt" ) ) {
                    commandVM.commands[ key ].iconId = "";
                }
            }
        }
        commandVM.commands[ "cmdGroupByTableOptions2" ].iconId = "";
        commandVM.commands[ id ].iconId = "cmdCheckmark";
        _appCtxSvc.updateCtx( 'momACBCurrentGroupBy', id );
        _appCtxSvc.updateCtx( 'ViewModeContext', viewMode );
    } );
};
exports.closeGroupByMenu = function() {
    var viewModeObj = _appCtxSvc.getCtx( 'ViewModeContext' );
    var commandsVMPromise = _utilsSvc.getCfg( 'commandsViewModel' );
    commandsVMPromise.then( function( commandVM ) {
        let viewMode = {
            "supportedViewModes": {
                "SummaryView": {},
                "TableSummaryView": {},
                "ListView": {},
                "TableView": {}
            },
            "ViewModeContext": "TableView"
        };
        for( let key in commandVM.commands ) {
            if( commandVM.commands.hasOwnProperty( key ) ) {
                if( key.includes( "groupByOpt" ) ) {
                    commandVM.commands[ key ].iconId = "";
                }
            }
        }
        commandVM.commands[ "cmdGroupByTableOptions2" ].iconId = "cmdCheckmark";
        viewMode.ViewModeContext = viewModeObj.ViewModeContext === "ListGroupView" ? "ListView" : "TableView";
        _appCtxSvc.updateCtx( 'ViewModeContext', viewMode );
    } );
};
exports.updateColumnsDataprovider = function( commandContext ) {
    var control = document.getElementById( "rightSidePanelList" );
    var customizeViewModel = _vMSvc.getViewModelUsingElement( control );
    var availableDp = customizeViewModel.dataProviders.leftSideDataProvider;
    var selectedDp = customizeViewModel.dataProviders.rightSideDataProvider;
    var vmo;
    var vmoArray = [];
    var vmoArrayAvailable = [];
    var vmoArraySelected = [];
    var popItem;
    switch ( commandContext ) {
        case "Right":
            if( availableDp.selectedObjects !== undefined && availableDp.selectedObjects.length > 0 ) {
                // Move Item into Selected Columns
                vmo = availableDp.selectedObjects[ 0 ];
                vmo.selected = false;
                vmoArray = selectedDp.getViewModelCollection().loadedVMObjects;
                vmoArray.push( vmo );
                selectedDp.update( vmoArray, vmoArray.length );
                //Remove from the Available Columns
                vmoArrayAvailable = availableDp.getViewModelCollection().loadedVMObjects;
                popItem = vmoArrayAvailable.find( x => x.uiValue === vmo.uiValue );
                vmoArrayAvailable.splice( vmoArrayAvailable.indexOf( popItem ), 1 );
                availableDp.update( vmoArrayAvailable, vmoArrayAvailable.length );
            }
            break;
        case "Left":
            // Move Item into Available Columns
            vmo = selectedDp.selectedObjects[ 0 ];
            vmo.selected = false;
            vmoArray = availableDp.getViewModelCollection().loadedVMObjects;
            vmoArray.push( vmo );
            availableDp.update( vmoArray, vmoArray.length );
            //Remove from the Selected Columns
            vmoArraySelected = selectedDp.getViewModelCollection().loadedVMObjects;
            popItem = vmoArraySelected.find( x => x.uiValue === vmo.uiValue );
            vmoArraySelected.splice( vmoArraySelected.indexOf( popItem ), 1 );
            selectedDp.update( vmoArraySelected, vmoArraySelected.length );

            break;
        case "RightAll":
            // Move All into Selected Columns
            vmoArrayAvailable = availableDp.getViewModelCollection().loadedVMObjects;
            vmoArraySelected = selectedDp.getViewModelCollection().loadedVMObjects;
            vmoArraySelected = vmoArraySelected.concat( vmoArrayAvailable );
            selectedDp.update( vmoArraySelected, vmoArraySelected.length );
            //Remove from the Available Columns
            availableDp.update( [], 0 );
            break;
        case "LeftAll":
            // Move All into Selected Columns
            vmoArraySelected = selectedDp.getViewModelCollection().loadedVMObjects;
            vmoArrayAvailable = availableDp.getViewModelCollection().loadedVMObjects;
            vmoArrayAvailable = vmoArrayAvailable.concat( vmoArraySelected );
            availableDp.update( vmoArrayAvailable, vmoArrayAvailable.length );
            //Remove from the Available Columns
            selectedDp.update( [], 0 );
            break;
        case "Reset":

            var columns;
            var resultAvailable = [];
            var resultSelected = [];
            if( momAcbConfiguration !== undefined ) {
                columns = momAcbConfiguration.tableProperties.columns.available;
                resultAvailable = [];
                columns.forEach( element => {
                    var item = _uwPropertySvc.createViewModelProperty( "revisionNumberValues", "", "STRING", [], element.name );
                    item.uiValue = element.uiValue;
                    resultAvailable.push( item );
                } );

                columns = momAcbConfiguration.tableProperties.columns.selected;
                columns.forEach( element => {
                    var item = _uwPropertySvc.createViewModelProperty( "revisionNumberValues", "", "STRING", [], element.name );
                    item.uiValue = element.uiValue;
                    resultSelected.push( item );
                } );

            }
            var merged = [ ...resultAvailable, ...resultSelected ];
            availableDp.update( merged, merged.length );
            selectedDp.update( [], 0 );
            break;
    }
};
exports.moveUpInList = function() {
    var control = document.getElementById( "rightSidePanelList" );
    var customizeViewModel = _vMSvc.getViewModelUsingElement( control );
    var availableDp = customizeViewModel.dataProviders.leftSideDataProvider;
    var selectedDp = customizeViewModel.dataProviders.rightSideDataProvider;
    if( selectedDp.selectedObjects[ 0 ] !== undefined ) {
        let moveObject = selectedDp.selectedObjects[ 0 ];
        let vmoArraySelected = selectedDp.getViewModelCollection().loadedVMObjects;
        let indexSelected = vmoArraySelected.indexOf( moveObject );
        vmoArraySelected = moveArrayItemToNewIndex( vmoArraySelected, indexSelected, indexSelected - 1 );
        selectedDp.update( vmoArraySelected, vmoArraySelected.length );
    } else if( availableDp.selectedObjects[ 0 ] !== undefined ) {
        let moveObject = availableDp.selectedObjects[ 0 ];
        let vmoArraySelected = availableDp.getViewModelCollection().loadedVMObjects;
        let indexSelected = vmoArraySelected.indexOf( moveObject );
        vmoArraySelected = moveArrayItemToNewIndex( vmoArraySelected, indexSelected, indexSelected - 1 );
        availableDp.update( vmoArraySelected, vmoArraySelected.length );
    }

};
exports.moveDownInList = function() {
    var control = document.getElementById( "rightSidePanelList" );
    var customizeViewModel = _vMSvc.getViewModelUsingElement( control );
    var availableDp = customizeViewModel.dataProviders.leftSideDataProvider;
    var selectedDp = customizeViewModel.dataProviders.rightSideDataProvider;
    if( selectedDp.selectedObjects[ 0 ] !== undefined ) {
        let moveObject = selectedDp.selectedObjects[ 0 ];
        let vmoArraySelected = selectedDp.getViewModelCollection().loadedVMObjects;
        let indexSelected = vmoArraySelected.indexOf( moveObject );
        vmoArraySelected = moveArrayItemToNewIndex( vmoArraySelected, indexSelected, indexSelected + 1 );
        selectedDp.update( vmoArraySelected, vmoArraySelected.length );
    } else if( availableDp.selectedObjects[ 0 ] !== undefined ) {
        let moveObject = availableDp.selectedObjects[ 0 ];
        let vmoArraySelected = availableDp.getViewModelCollection().loadedVMObjects;
        let indexSelected = vmoArraySelected.indexOf( moveObject );
        vmoArraySelected = moveArrayItemToNewIndex( vmoArraySelected, indexSelected, indexSelected + 1 );
        availableDp.update( vmoArraySelected, vmoArraySelected.length );
    }
};
exports.filterAction = function( filterInput ) {
    var control = document.getElementById( filterInput.vm );
    var controlVM = _vMSvc.getViewModelUsingElement( control );

    filterInput.contextData.columnObject.filter.textValue = filterInput.contextData.textValue;
    filterInput.contextData.columnObject.filter.operation.dbValue = "contains";
    controlVM.context = {
        column: filterInput.contextData.columnObject,
        columnProvider: controlVM.columnProviders[ momAcbConfiguration.tableProperties.columnProvider ],
        dataProvider: controlVM.dataProviders[ momAcbConfiguration.tableProperties.dataProvider ],
        filterError: false,
        filterNoAction: false
    };
    if( !isFirstTime() ) {
        _awColFilterSrv.doFiltering( controlVM.context.column, controlVM.context.columnProvider, controlVM.context.dataProvider, controlVM );
        _appCtxSvc.updateCtx( filterInput.vm + "_criteria", controlVM.context.columnProvider.columnFilters );
        _eventBus.publish( filterInput.vm + ".plTable.reload" );
    }
};
exports.doSearch = function( filterInput ) {
    var currentMode = _viewModeService.getViewMode();
    var searchCriteria = "";
    var isTable = currentMode === 'TableView' || currentMode === 'TableTreeView' || currentMode === 'SummaryView' ? true : false;
    if( isTable ) {
        searchCriteria = momAcbConfiguration.tableProperties.searchCriteria;
        var control = document.getElementById( momAcbConfiguration.tableProperties.id );
        var controlVM = _vMSvc.getViewModelUsingElement( control );
        var dataProvider = controlVM.dataProviders[ momAcbConfiguration.tableProperties.dataProvider ];
        var columnObject = dataProvider.cols.find( x => x.name === searchCriteria );
        columnObject.filter.textValue.dbValue = filterInput.searchInput.dbValue;
        columnObject.filter.operation.dbValue = "contains";
        controlVM.context = {
            column: columnObject,
            columnProvider: controlVM.columnProviders[ momAcbConfiguration.tableProperties.columnProvider ],
            dataProvider: controlVM.dataProviders[ momAcbConfiguration.tableProperties.dataProvider ],
            filterError: false,
            filterNoAction: false
        };
        _awColFilterSrv.doFiltering( controlVM.context.column, controlVM.context.columnProvider, controlVM.context.dataProvider, controlVM );
        _appCtxSvc.updateCtx( momAcbConfiguration.tableProperties.id + "_criteria", controlVM.context.columnProvider.columnFilters );
        _eventBus.publish( momAcbConfiguration.tableProperties.id + ".plTable.reload" );
    } else {
        searchCriteria = momAcbConfiguration.listProperties.searchCriteria;
        control = document.getElementById( momAcbConfiguration.listProperties.id );
        controlVM = _vMSvc.getViewModelUsingElement( control );
        var filterEntry = {};
        if( controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB !== undefined ) {
            if( controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB.some( e => e.propName === filterInput.searchCriteria ) ) {
                var oldFilterEntry = controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB.filter( e => e.propName === filterInput.searchCriteria );
                controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB =
                    controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB.filter( x => x.propName !== oldFilterEntry[ 0 ].propName );
            }
            filterEntry = {
                propName: searchCriteria,
                propValue: filterInput.searchInput.dbValue
            };
            controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB.push( filterEntry );
        } else {

            controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB = [];
            filterEntry = {
                propName: searchCriteria,
                propValue: filterInput.searchInput.dbValue
            };
            controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB.push( filterEntry );
        }
        _eventBus.publish( momAcbConfiguration.listProperties.view + '.contentLoaded' );
    }
};
exports.filterActionSearch = function( filterInput ) {
    var control = document.getElementById( filterInput.vm );
    var controlVM = _vMSvc.getViewModelUsingElement( control );

    filterInput.contextData.columnObject.filter.textValue = filterInput.contextData.textValue;
    filterInput.contextData.columnObject.filter.operation.dbValue = "contains";
    controlVM.context = {
        column: filterInput.contextData.columnObject,
        columnProvider: controlVM.columnProviders[ momAcbConfiguration.tableProperties.columnProvider ],
        dataProvider: controlVM.dataProviders[ momAcbConfiguration.tableProperties.dataProvider ],
        filterError: false,
        filterNoAction: false
    };
    if( !isFirstTime() ) {
        _awColFilterSrv.doFiltering( controlVM.context.column, controlVM.context.columnProvider, controlVM.context.dataProvider, controlVM );
        _eventBus.publish( filterInput.vm + ".plTable.reload" );
    }
};
exports.sortAction = function( sortInput ) {
    var commandsVMPromise = _utilsSvc.getCfg( 'commandsViewModel' );

    commandsVMPromise.then( function( commandVM ) {
        var control = document.getElementById( sortInput.vm );
        var controlVM = _vMSvc.getViewModelUsingElement( control );
        var toggle = momAcbConfiguration.tableProperties.sortDirection;

        toggle = toggle === undefined ? true : toggle;
        controlVM.columnProviders[ momAcbConfiguration.tableProperties.columnProvider ].sortCriteria = [ {
            fieldName: sortInput.columnName,
            sortDirection: toggle ? "ASC" : "DESC"
        } ];
        controlVM.columns.forEach( column => {
            if( column.name !== 'icon' ) {
                commandVM.commands[ column.name ].iconId = "";
            }
        } );
        commandVM.commands[ sortInput.columnName ].iconId = toggle ? "cmdUpArrow" : "cmdDownArrow";
        momAcbConfiguration.tableProperties.sortDirection = !toggle;
        _appCtxSvc.updateCtx( 'momACBCurrentSort', sortInput.columnName );
        _eventBus.publish( sortInput.vm + ".plTable.reload" );
    } );
};
exports.sortListAction = function( sortInput ) {
    var commandsVMPromise = _utilsSvc.getCfg( 'commandsViewModel' );

    commandsVMPromise.then( function( commandVM ) {
        var control = document.getElementById( sortInput.vm );
        var controlVM = _vMSvc.getViewModelUsingElement( control );
        var sortCriteria = controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].sortCriteria;
        var toggle = true;
        if( sortCriteria.some( e => e.fieldName === sortInput.columnName ) ) {
            var oldSortEntry = sortCriteria.filter( e => e.fieldName === sortInput.columnName )[ 0 ];
            toggle = oldSortEntry.sortDirection === "ASC" ? false : true;
            // sortCriteria = sortCriteria.filter(x=>x.fieldName !== oldSortEntry.fieldName);
            sortCriteria.pop( oldSortEntry );
        }
        sortCriteria.forEach( column => {
            if( column.fieldName !== 'icon' ) {
                commandVM.commands[ column.fieldName ].iconId = "";
            }
        } );
        controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].sortCriteria = [];
        controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].sortCriteria = [ {
            fieldName: sortInput.columnName,
            sortDirection: toggle ? "ASC" : "DESC"
        } ];
        commandVM.commands[ sortInput.columnName ].iconId = toggle ? "cmdUpArrow" : "cmdDownArrow";
        _appCtxSvc.updateCtx( 'momACBCurrentSort', sortInput.columnName );
        _eventBus.publish( momAcbConfiguration.listProperties.view + '.contentLoaded' );
    } );
};
exports.switchViewModeForACB = function( input ) {

    var toggle = _appCtxSvc.getCtx( 'switchModesIsSelected' );
    _appCtxSvc.updateCtx( 'switchModesIsSelected', !toggle );
    var currentMode = _viewModeService.getViewMode();
    switch ( currentMode ) {
        case 'ListView':
            currentMode = 'TableView';
            break;
        case 'TableView':
            currentMode = 'ListView';
            break;
        case 'ListGroupView':
            currentMode = 'TableTreeView';
            break;
        case 'TableTreeView':
            currentMode = 'ListGroupView';
            break;
        case 'SummaryView':
            currentMode = 'ListView';
            break;
        default:
            currentMode = 'TableView';
            break;
    }
    let viewMode = {
        "supportedViewModes": {
            "SummaryView": {},
            "TableSummaryView": {},
            "ListView": {},
            "TableView": {}
        },
        "ViewModeContext": currentMode
    };
    _appCtxSvc.updateCtx( 'ViewModeContext', viewMode );

    this.openStandardSortMenu();
    this.openStandardGroupMenu();
};
exports.filterListAction = function( filterInput ) {
    var control = document.getElementById( filterInput.vm );
    var controlVM = _vMSvc.getViewModelUsingElement( control );

    var filterEntry = {};
    if( !isFirstTime() ) {
        if( controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB !== undefined ) {
            if( controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB.some( e => e.propName === filterInput.contextData.columnName ) ) {
                var oldFilterEntry = controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB.filter( e => e.propName === filterInput.contextData.columnName );
                controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB =
                    controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB.filter( x => x.propName !== oldFilterEntry[ 0 ].propName );
            }
            filterEntry = {
                propName: filterInput.contextData.columnName,
                propValue: filterInput.contextData.textValue.dbValue
            };
            controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB.push( filterEntry );
        } else {

            controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB = [];
            filterEntry = {
                propName: filterInput.contextData.columnName,
                propValue: filterInput.contextData.textValue.dbValue
            };
            controlVM.dataProviders[ momAcbConfiguration.listProperties.dataProvider ].filtersFromACB.push( filterEntry );
        }
        _eventBus.publish( momAcbConfiguration.listProperties.view + '.contentLoaded' );
    }
};
exports.createViewViewModelFromColumns = function() {
    var control = document.getElementById( momAcbConfiguration.tableProperties.id );
    var controlVM = _vMSvc.getViewModelUsingElement( control );
    var viewModel = {
        schemaVersion: "1.0.0",
        imports: [
            "js/aw-column.directive",
            "js/aw-row.directive",
            "js/aw-textbox.directive"
        ],
        data: {},
        actions: {},
        conditions: {},
        onEvent: []
    };
    var view = document.createElement( "aw-row" );
    view.setAttribute( "height", "5f" );
    var dataProvider = controlVM.dataProviders[ momAcbConfiguration.tableProperties.dataProvider ];
    controlVM.columns.forEach( column => {
        if( column.name !== 'icon' ) {
            // Modifying View
            var columnContainer = document.createElement( "aw-column" );

            var fillerColumn = document.createElement( "aw-column" );
            fillerColumn.setAttribute( "width", "1f" );

            var searchField = document.createElement( "aw-textbox" );
            searchField.setAttribute( "prop", "data." + column.name );

            columnContainer.appendChild( searchField );
            view.appendChild( columnContainer );
            view.appendChild( fillerColumn );

            // Modifying ViewModel
            viewModel.data[ column.name ] = {
                displayName: column.displayName,
                type: "STRING",
                isRequired: "false",
                isEditable: "true",
                dbValue: "",
                dispValue: ""
            };
            viewModel.actions[ column.name + "Action" ] = {
                actionType: "JSFunction",
                method: "filterAction",
                inputData: {
                    filterInput: {
                        vm: momAcbConfiguration.tableProperties.id,
                        contextData: {
                            columnName: column.name,
                            operation: "contains",
                            textValue: "{{data." + column.name + "}}",
                            columnObject: dataProvider.cols.find( x => x.name === column.name )
                        }
                    }
                },
                deps: "js/mom.acb.component.service"
            };
            viewModel.onEvent.push( {
                eventId: "condition.expressionValueChanged",
                action: column.name + "Action",
                eventSource: "current",
                criteria: {
                    condition: "conditions." + column.name + "Changed"
                },
                cacheEventData: true
            } );
            viewModel.conditions[ column.name + "Changed" ] = {
                expression: "data." + column.name + ".dbValue",
                trackValues: true
            };
            viewModel.conditions[ column.name + "ValueUpdated" ] = {
                expression: "data." + column.name + ".valueUpdated",
                trackValues: true
            };
        }
    } );

    return {
        view: view,
        viewModel: viewModel
    };
};
exports.createViewViewModelFromList = function() {
    var viewModel = {
        schemaVersion: "1.0.0",
        imports: [
            "js/aw-column.directive",
            "js/aw-row.directive",
            "js/aw-textbox.directive"
        ],
        data: {},
        actions: {},
        conditions: {},
        onEvent: []
    };
    var view = document.createElement( "aw-row" );
    view.setAttribute( "height", "5f" );
    var props = momAcbConfiguration.listProperties.propDefinitions.propNames;
    // update order
    let propsOrder = momAcbConfiguration.listProperties.propDefinitions.props;
    let listOrder = [];
    propsOrder.forEach( element => {
        listOrder.push( element.acbListBox.dbValue );
    } );
    var finalProps = [];
    listOrder.forEach( orderElem => {
        let pushItem = props.find( x => x.internalName === orderElem );
        if( pushItem !== undefined ) {
            finalProps.push( pushItem );
        }
    } );
    for( let prop in finalProps ) {
        var column = finalProps[ prop ];
        if( column.internalName !== 'icon' && column.internalName !== 'none' ) {
            // Modifying View
            var columnContainer = document.createElement( "aw-column" );

            var fillerColumn = document.createElement( "aw-column" );
            fillerColumn.setAttribute( "width", "1f" );

            var searchField = document.createElement( "aw-textbox" );
            searchField.setAttribute( "prop", "data." + column.internalName );

            columnContainer.appendChild( searchField );
            view.appendChild( columnContainer );
            view.appendChild( fillerColumn );

            // Modifying ViewModel
            viewModel.data[ column.internalName ] = {
                displayName: column.displayName,
                type: "STRING",
                isRequired: "false",
                isEditable: "true",
                dbValue: "",
                dispValue: ""
            };
            viewModel.actions[ column.internalName + "Action" ] = {
                actionType: "JSFunction",
                method: "filterListAction",
                inputData: {
                    filterInput: {
                        vm: momAcbConfiguration.listProperties.id,
                        contextData: {
                            columnName: column.internalName,
                            operation: "contains",
                            textValue: "{{data." + column.internalName + "}}",
                            columnObject: column
                        }
                    }
                },
                deps: "js/mom.acb.component.service"
            };
            viewModel.onEvent.push( {
                eventId: "condition.expressionValueChanged",
                action: column.internalName + "Action",
                criteria: {
                    condition: "conditions." + column.internalName + "Changed"
                },
                cacheEventData: true
            } );
            viewModel.conditions[ column.internalName + "Changed" ] = {
                expression: "data." + column.internalName + ".dbValue",
                trackValues: true
            };

        }
    }
    return {
        view: view,
        viewModel: viewModel
    };
};
exports.updateAcbConfig = function( popupId, targetEvent, defaultViewPref ) {
    //Get Data from the Table and List Properties (do both)
    //Table Selected and Available Values
    var control = document.getElementById( "rightSidePanelList" );
    var customizeViewModel = _vMSvc.getViewModelUsingElement( control );
    var availableArray = customizeViewModel.dataProviders.leftSideDataProvider.getViewModelCollection().loadedVMObjects;
    var selectedArray = customizeViewModel.dataProviders.rightSideDataProvider.getViewModelCollection().loadedVMObjects;
    var selectedObjectArray = [];
    var availableObjectArray = [];
    // Get values
    selectedArray.forEach( element => {
        var entity = {
            name: element.displayValues,
            uiValue: element.uiValue
        };
        selectedObjectArray.push( entity );
    } );
    availableArray.forEach( element => {
        var entity = {
            name: element.displayValues,
            uiValue: element.uiValue
        };
        availableObjectArray.push( entity );
    } );
    if( momAcbConfiguration !== undefined ) {
        momAcbConfiguration.tableProperties.columns.selected = selectedObjectArray;
        momAcbConfiguration.tableProperties.columns.available = availableObjectArray;
        momAcbConfiguration.isDefaultViewTable = defaultViewPref.dbValue;

        _appCtxSvc.updateCtx( 'momAcbConfiguration', momAcbConfiguration );
        let isFilterOpen = _appCtxSvc.getCtx( 'simpleFilterIsSelected' );
        if( isFilterOpen ) {
            this.openSimpleFilter();
        }
        // Close Pop-up
        _popupSvc.hide( popupId, targetEvent );
    }
};
exports.updateAcbConfigForList = function( popupId, targetEvent, defaultViewPref ) {
    //Get Data from the Table and List Properties (do both)
    //Table Selected and Available Values
    var control = document.getElementById( "rightSidePanelList" );
    var customizeViewModel = _vMSvc.getViewModelUsingElement( control );
    if( momAcbConfiguration !== undefined ) {
        // update list objects

        momAcbConfiguration.listProperties.propDefinitions.props = customizeViewModel.listDataObject;
        momAcbConfiguration.isDefaultViewTable = defaultViewPref.dbValue;
        // update order
        var props = momAcbConfiguration.listProperties.propDefinitions.propNames;
        let propsOrder = momAcbConfiguration.listProperties.propDefinitions.props;
        let listOrder = [];
        propsOrder.forEach( element => {
            listOrder.push( element.acbListBox.dbValue );
        } );
        var finalProps = [];
        listOrder.forEach( orderElem => {
            let pushItem = props.find( x => x.internalName === orderElem );
            if( pushItem !== undefined ) {
                finalProps.push( pushItem );
            }
        } );
        momAcbConfiguration.listProperties.propDefinitions.propNames = finalProps;
        _appCtxSvc.updateCtx( 'momAcbConfiguration', momAcbConfiguration );
        let isFilterOpen = _appCtxSvc.getCtx( 'simpleFilterIsSelected' );
        if( isFilterOpen ) {
            this.openSimpleFilter();
        }
        // Close Pop-up
        _popupSvc.hide( popupId, targetEvent );
    }
};
exports.generatePreview = function() {
    //Table Selected and Available Values
    var listOrder = {};
    var props = {};
    // Get the Config

    if( momAcbConfiguration !== undefined ) {
        props = momAcbConfiguration.listProperties.propDefinitions.props;
        listOrder = {};
        props.forEach( element => {
            listOrder[ element.acbListBox.displayName ] = element.acbListBox.dbValue;
        } );
    }
    return { preview: listOrder };
};
exports.reloadPreviewPage = function() {
    var control = document.getElementById( "rightSidePanelList" );
    var customizeViewModel = _vMSvc.getViewModelUsingElement( control );
    // Get the Config

    if( momAcbConfiguration !== undefined ) {
        momAcbConfiguration.listProperties.propDefinitions.props = customizeViewModel.listDataObject;
        _appCtxSvc.updateCtx( 'momAcbConfiguration', momAcbConfiguration );
        _eventBus.publish( "momACBPreview.contentLoaded" );
    }
};
exports.resetListConfigPage = function() {
    var momACBConfigPromise = _configSvc.getCfg( 'momAcbConfiguration', true );
    momACBConfigPromise.then( function( momACBConfig ) {

        momAcbConfiguration.listProperties.propDefinitions.props = momACBConfig.listProperties.propDefinitions.props;
        _eventBus.publish( "acbListSettings.contentLoaded" );
    } );
};

// Compact Menu Methods

exports.openCompactSortMenu = function() {
    var controlId = momAcbConfiguration.compactACB.id;
    var commandsVMPromise = _utilsSvc.getCfg( 'commandsViewModel' );
    commandsVMPromise.then( function( commandVM ) {
        var control = document.getElementById( controlId );
        var controlVM = _vMSvc.getViewModelUsingElement( control );
        var dataProvider = controlVM.dataProviders[ momAcbConfiguration.compactACB.dataProvider ];
        var props = dataProvider.viewModelCollection.loadedVMObjects[ 0 ].props;
        for( let prop in props ) {
            var column = props[ prop ];
            if( column.propertyName !== 'icon' ) {
                // Add command in commands
                commandVM.commands[ "compactSort" + column.propertyName ] = {
                    title: column.propertyDisplayName
                };
                // Add command Handler
                commandVM.commandHandlers[ "compactSort" + column.propertyName + "Handler" ] = {
                    id: "compactSort" + column.propertyName,
                    action: "compactSort" + column.propertyName + "Action",
                    activeWhen: true,
                    visibleWhen: true,
                    selectedWhen: "ctx.momACBCurrentSort === " + column.propertyName

                };
                // Add command Placements
                commandVM.commandPlacements[ "compactSort" + column.propertyName + "Placement" ] = {
                    id: "compactSort" + column.propertyName,
                    uiAnchor: "momCompactACBDefaultCommands",
                    priority: 30,
                    parentGroupId: "cmdCompactBarSort",
                    showGroupSelected: true
                };
                // Add command Actions
                commandVM.actions[ "compactSort" + column.propertyName + "Action" ] = {
                    actionType: "JSFunction",
                    method: "sortListCompactAction",
                    inputData: {
                        sortInput: {
                            vm: controlId,
                            columnName: column.propertyName,
                            isAscending: true

                        }
                    },
                    deps: "js/mom.acb.component.service"
                };
            }
        }
        _utilsSvc.updateCommands( commandVM );
    } );
};

exports.doCompactSearch = function( filterInput ) {
    var control = document.getElementById( momAcbConfiguration.compactACB.id );
    var controlVM = _vMSvc.getViewModelUsingElement( control );
    controlVM.dataProviders[ momAcbConfiguration.compactACB.dataProvider ].filtersFromACB = [];
    var filterEntry = {
        propName: momAcbConfiguration.compactACB.searchCriteria,
        propValue: filterInput.searchInput.dbValue
    };
    controlVM.dataProviders[ momAcbConfiguration.compactACB.dataProvider ].filtersFromACB.push( filterEntry );
    _eventBus.publish( momAcbConfiguration.compactACB.id + '.contentLoaded' );
};

exports.sortListCompactAction = function( sortInput ) {
    var commandsVMPromise = _utilsSvc.getCfg( 'commandsViewModel' );
    commandsVMPromise.then( function( commandVM ) {
        var control = document.getElementById( sortInput.vm );
        var controlVM = _vMSvc.getViewModelUsingElement( control );
        var sortDp = controlVM.dataProviders[ momAcbConfiguration.compactACB.dataProvider ];
        var sortCriteria = sortDp.sortCriteria;
        var toggle = true;
        if( sortCriteria.some( e => e.fieldName === sortInput.columnName ) ) {
            var oldSortEntry = sortCriteria.filter( e => e.fieldName === sortInput.columnName )[ 0 ];
            toggle = oldSortEntry.sortDirection === "ASC" ? false : true;
            // sortCriteria = sortCriteria.filter(x=>x.fieldName !== oldSortEntry.fieldName);
            sortCriteria.pop( oldSortEntry );
        }
        sortCriteria.forEach( column => {
            if( column.fieldName !== 'icon' ) {
                commandVM.commands[ "compactSort" + column.fieldName ].iconId = "";
            }
        } );
        controlVM.dataProviders[ momAcbConfiguration.compactACB.dataProvider ].sortCriteria = [];
        controlVM.dataProviders[ momAcbConfiguration.compactACB.dataProvider ].sortCriteria = [ {
            fieldName: sortInput.columnName,
            sortDirection: toggle ? "ASC" : "DESC"
        } ];
        commandVM.commands[ "compactSort" + sortInput.columnName ].iconId = toggle ? "cmdUpArrow" : "cmdDownArrow";
        _appCtxSvc.updateCtx( 'momACBCurrentSort', sortInput.columnName );
        if( toggle ) {
            sortDp.viewModelCollection.loadedVMObjects.sort( ( a, b ) =>
                ( a.props.find( x => x.propertyName === sortInput.columnName ).dbValue > b.props.find( x => x.propertyName === sortInput.columnName ).dbValue ) ? 1 : -1 );
        } else {
            sortDp.viewModelCollection.loadedVMObjects.sort( ( a, b ) =>
                ( a.props.find( x => x.propertyName === sortInput.columnName ).dbValue < b.props.find( x => x.propertyName === sortInput.columnName ).dbValue ) ? 1 : -1 );
        }
        controlVM.dataProviders[ momAcbConfiguration.compactACB.dataProvider ].update( sortDp.viewModelCollection.loadedVMObjects, sortDp.viewModelCollection.loadedVMObjects.length );
    } );
};

exports.searchCompactACB = function() {
    var flag = _appCtxSvc.getCtx( 'openCompactSearchBar' );
    flag = flag !== undefined ? flag : false;
    _appCtxSvc.updateCtx( 'openCompactSearchBar', !flag );
};

// Private methods

var moveArrayItemToNewIndex = function( arr, old_index, new_index ) {
    if( new_index >= arr.length ) {
        var k = new_index - arr.length + 1;
        while( k-- ) {
            arr.push( undefined );
        }
    }
    arr.splice( new_index, 0, arr.splice( old_index, 1 )[ 0 ] );
    return arr;
};
var cleanCommandVM = function( commandVm, identifier ) {
    for( let key in commandVm.commands ) {
        if( commandVm.commands.hasOwnProperty( key ) ) {
            if( commandVm.commands[ key ].identifier === identifier ) {
                delete commandVm.commands[ key ];
            }
        }
    }
    for( let key in commandVm.commandHandlers ) {
        if( commandVm.commandHandlers.hasOwnProperty( key ) ) {
            if( commandVm.commandHandlers[ key ].identifier === identifier ) {
                delete commandVm.commandHandlers[ key ];
            }
        }
    }
    for( let key in commandVm.commandPlacements ) {
        if( commandVm.commandPlacements.hasOwnProperty( key ) ) {
            if( commandVm.commandPlacements[ key ].identifier === identifier ) {
                delete commandVm.commandPlacements[ key ];
            }
        }
    }
};
app.factory( 'mom.acb.component.service', () => exports );

export default exports;
