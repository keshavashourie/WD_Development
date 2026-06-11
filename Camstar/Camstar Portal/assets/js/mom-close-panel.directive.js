// Copyright 2020 Siemens AG

/*global
 define
 */
/* eslint-disable valid-jsdoc */
/**
 * A module exposing a custom element used as a touch friendly input.
 * @module "js/mom-close-panel.directive"
 * @ignore
 */
import app from 'app';
import 'js/aw-repeat.directive';
import 'js/exist-when.directive';
import 'js/aw-pic.directive';
import _ from 'lodash';
import "js/enable-when.directive";
import _utilsSvc from 'js/mom.utils.service';
import 'js/appCtxService';

/**
 * A custom Panel to close opened Tabs for a specific location or
 * @typedef "mom-close-panel.directive"
 * @property {Expression} prop Property to bind with the control
 * @implements {Element}
 * @example
 * <mom-close-panel prop="'data.prop'"></mom-close-panel>
 */
app.directive( 'momClosePanel', [
    'viewModelService', 'localeService', 'messagingService', 'appCtxService',
    function( viewModelSvc, _localeService, msgSvc, _appCtxSvc ) {
        return {
            restrict: 'E',
            scope: {
                prop: '=',
                commands: '=?'
            },
            controller: [ '$scope', '$rootScope', function( $scope, $rootScope ) {
                var declViewModel = viewModelSvc.getViewModel( $scope, true );
                viewModelSvc.bindConditionStates( declViewModel, $scope );
                $scope.conditions = declViewModel.getConditionStates();
                // Set the Scope
                var stateConfig = _utilsSvc.getCfg( "states" );
                var finalResult = {};
                stateConfig.then( function name( result ) {
                    var visitedStatesList = _appCtxSvc.getCtx( "visitedStatesList" );
                    // var visitedStatesList = [ "locationA", "locationB", "locationD", "notes" ];
                    visitedStatesList.forEach( visitedSate => {
                        if( result[ visitedSate ] !== undefined ) {
                            var stateObject = result[ visitedSate ];
                            var parentObject = result[ stateObject.parent ];
                            var parentObjectName = parentObject.data.headerTitle;
                            //create the ParentNode if that doesn't exist
                            if( finalResult[ parentObjectName ] === undefined ) {
                                finalResult[ parentObjectName ] = {
                                    name: parentObjectName,
                                    states: []
                                };
                            }
                            //create CheckList
                            var localizedName = _localeService.getLocalizedText( stateObject.data.label.source, stateObject.data.label.key ).$$state.value;
                            stateObject.localizedName = localizedName;
                            finalResult[ parentObjectName ].states.push( stateObject );
                        }
                    } );
                    $scope.visitedStatesResults = finalResult;
                } );
                //Set the CheckMark
                $scope.CurrentState = $scope.ctx.locationContext[ "ActiveWorkspace:SubLocation" ];
                $scope.buttonCommand = {
                    dbValue: "cmdCloseTab"
                };
                $scope.buttonCheckMark = {
                    dbValue: "cmdCheckmark"
                };
                $scope.closeAll = {
                    dbValue: "cmdCloseAllTabs"
                };
                $scope.CloseLocation = function( name, state ) {
                    if( $scope.CurrentState === state.name ) {
                        msgSvc.showWarning( "This is the active tab, do you want close active tab and lose changes? ", [ {
                                id: "yes",
                                text: "Yes",
                                addClass: "btn btn-notify",
                                onClick: ( $noty ) => {
                                    $noty.close();
                                    updateStateInTabs( $scope, viewModelSvc, _appCtxSvc, state, name );
                                    return Promise.resolve( { buttonId: "yes" } );
                                }
                            },
                            {
                                id: 'no',
                                text: 'No',
                                addClass: "btn btn-notify",
                                onClick: ( $noty ) => {
                                    $noty.close();
                                    return Promise.resolve( { buttonId: "no" } );
                                }
                            }
                        ], "", "" );
                    } else {
                        updateStateInTabs( $scope, viewModelSvc, _appCtxSvc, state, name );
                    }
                };
                $scope.CloseAllLocationsFromState = function( name, states ) {
                    msgSvc.showWarning( "Do you want to close all open tabs for " + states.name + "?", [ {
                            id: "yes",
                            text: "Yes",
                            addClass: "btn btn-notify",
                            onClick: ( $noty ) => {
                                $noty.close();
                                this.CloseAllTabs( name, states );
                                return Promise.resolve( { buttonId: "yes" } );
                            }
                        },
                        {
                            id: 'no',
                            text: 'No',
                            addClass: "btn btn-notify",
                            onClick: ( $noty ) => {
                                $noty.close();
                                return Promise.resolve( { buttonId: "no" } );
                            }
                        }
                    ], "", "" );
                };
                $scope.navigateToState = function( state ) {
                    _utilsSvc.navigateTo( state );
                };
                $scope.CloseAllTabs = function( name, states ) {
                    let statesLength = states.states.length;
                    for( let index = 0; index < statesLength; index++ ) {
                        if( states.states.length > 1 ) {
                            updateStateInTabs( $scope, viewModelSvc, _appCtxSvc, states.states[ index ], name );
                        } else {
                            updateStateInTabs( $scope, viewModelSvc, _appCtxSvc, states.states[ 0 ], name );
                        }
                    }
                    // Navigation to the Next Location First Tab
                    let firstKey = Object.keys( $scope.visitedStatesResults )[ 0 ];
                    this.navigateToState( $scope.visitedStatesResults[ firstKey ].states[ 0 ] );
                };
            } ],
            replace: true,
            templateUrl: app.getBaseUrlPath() + '/html/mom-close-panel.directive.html',
            link: function( scope, element ) {}
        };
    }
] );
/* eslint-disable*/
function updateStateInTabs( scope, viewModelSvc, _appCtxSvc, state, name ) {
    //Manage the VM
    let tabContainer = document.getElementsByClassName( 'aw-jswidget-tabContainer' );
    let tabContainerScope = angular.element( tabContainer );
    let declViewModel = viewModelSvc.getViewModel( tabContainerScope.scope(), true );
    declViewModel.subLocationTabs = declViewModel.subLocationTabs.filter( function( tab ) { return tab.state !== state.name; } )

    // retrieve the scope and VM for the Tabs
    let listOfTabButtons = document.querySelectorAll( "#stdCloseButton" );
    let listOfTabButtonsArray = Array.prototype.slice.call( listOfTabButtons );
    let tabToClose = listOfTabButtonsArray.filter( function( button ) { return button.parentElement.innerText === state.localizedName; } )
    let angularButton = angular.element( tabToClose );
    if( angularButton.length > 0 ) {
        angularButton.scope().$parent.closeClickTab();
    }

    // remove from the Panel List
    let visitedSatesList = _appCtxSvc.getCtx( 'visitedStatesList' );
    var indexCtx = visitedSatesList.indexOf( state.name );
    visitedSatesList.splice( indexCtx, 1 );
    _appCtxSvc.registerCtx( 'visitedStatesList', visitedSatesList );

    indexCtx = scope.visitedStatesResults[ name ].states.indexOf( state );
    scope.visitedStatesResults[ name ].states.splice( indexCtx, 1 );
    if( scope.visitedStatesResults[ name ].states.length < 1 ) {
        delete scope.visitedStatesResults[ name ]
    }
}
