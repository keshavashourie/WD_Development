// Copyright 2020 Siemens AG

/*global
 define
 */
/* eslint-disable valid-jsdoc */
/**
 * A module exposing a custom element used as a touch friendly input.
 * @module "js/mom-numeric-input.directive"
 * @ignore
 */
import app from 'app';
import 'js/aw-property-label.directive';
import 'js/aw-property-non-edit-val.directive';
import 'js/aw-numeric.directive';
import 'js/exist-when.directive';
import 'js/aw-icon-button.directive';
import uwPropertySvc from 'js/uwPropertyService';
import _ from 'lodash';
import eventBus from 'js/eventBus';

/**
 * A custom element representing a element to be used for touch friendly numeric input.
 * @typedef "mom-numeric-input"
 * @property {Expression} prop Property to bind with the control
 * @implements {Element}
 * @example
 * <mom-numeric-input prop="'data.prop'"></mom-numeric-input>
 */
app.directive( 'momNumericInput', [
    'viewModelService',
    function( viewModelSvc ) {
        return {
            restrict: 'E',
            scope: {
                prop: '=',
                customCommands: '@',
                commands: '=?'
            },
            controller: [ '$scope', function( $scope ) {
                var declViewModel = viewModelSvc.getViewModel( $scope, true );
                viewModelSvc.bindConditionStates( declViewModel, $scope );
                $scope.conditions = declViewModel.getConditionStates();
                // Set Custom Commands
                $scope.customCommands = declViewModel.momNumericInputCommands !== undefined ? declViewModel.momNumericInputCommands.find(x=>x.propertyName === $scope.prop.propertyName) : undefined;
                if( $scope.customCommands !== undefined ) {
                    if( $scope.customCommands.leftCommands !== undefined ) {
                        $scope.leftCommands = $scope.customCommands.leftCommands;
                    } else {
                        $scope.leftCommands = [ {
                            iconName: "miscDecrease_uxRefresh",
                            tooltip: "Remove"
                        } ];
                    }
                    if( $scope.customCommands.rightCommands !== undefined ) {
                        $scope.rightCommands = $scope.customCommands.rightCommands;
                    } else {
                        $scope.rightCommands = [ {
                            iconName: "miscIncrease_uxRefresh",
                            tooltip: "Add",
                            isDefault: true
                        } ];
                    }
                } else {
                    $scope.leftCommands = [ {
                        iconName: "miscDecrease_uxRefresh",
                        tooltip: "Remove",
                        isDefault: true
                    } ];
                    $scope.rightCommands = [ {
                        iconName: "miscIncrease_uxRefresh",
                        tooltip: "Add",
                        isDefault: true
                    } ];
                }

                // initialize all default command condition to true
                _.forEach( $scope.leftCommands, function( command ) {
                    if( command.condition === undefined ) {
                        command.condition = true;
                    }
                } );
                _.forEach( $scope.rightCommands, function( command ) {
                    if( command.condition === undefined ) {
                        command.condition = true;
                    }
                } );
            } ],
            replace: true,
            templateUrl: app.getBaseUrlPath() + '/html/mom-numeric-input.directive.html',
            link: function( scope, element ) {
                element.find( ':first' ).on( 'click', function( event ) {
                    var sourceButton = getButtonSource( event.target );
                    if( scope.leftCommands[ 0 ].isDefault || scope.rightCommands[ 0 ].isDefault ) {
                        if( sourceButton.isValid ) {
                            commandDefaultBehaviour( scope, sourceButton.button );
                            eventBus.publish( 'momNumericInput.command.clicked', {
                                buttonClicked: sourceButton.button,
                                isDefaultBehavior: true
                            } );
                        }
                    } else {
                        if( sourceButton.isValid ) {
                            eventBus.publish( 'momNumericInput.command.clicked', {
                                buttonClicked: sourceButton.button,
                                isDefaultBehavior: false
                            } );
                        }
                    }
                } );
            }
        };
    }
] );
/* eslint-disable*/
function commandDefaultBehaviour( scope, operator ) {
    var oldValue = scope.prop.dbValue;
    var newValue = operator === 'leftCommand' ? oldValue - 1 : oldValue + 1;
    uwPropertySvc.setValue( scope.prop, newValue );
    uwPropertySvc.setDisplayValue( scope.prop, [ newValue ] );
    uwPropertySvc.replaceValuesWithNewValues( scope.prop );
}
/* eslint-disable*/
function getButtonSource( source ) {
    var sourceButton = {};
    if( source !== undefined ) {
        switch ( source.tagName ) {
            case 'BUTTON':
                sourceButton.button = source.id;
                sourceButton.isValid = true;
                break;
            case 'svg':
            case 'path':
            case 'line':
            case 'g':
                sourceButton.button = source.parentElement.id;
                sourceButton.isValid = true;
                break;
            default:
                sourceButton.isValid = false;
                break;
        }
    }
    return sourceButton;
}
