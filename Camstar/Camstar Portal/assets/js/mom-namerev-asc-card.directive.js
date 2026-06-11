// Copyright 2020 Siemens AG

/*global
 define
 */
/* eslint-disable valid-jsdoc */
/**
 * A module exposing a custom element used as a touch friendly input.
 * @module "js/mom-namerev-asc-card.directive"
 * @ignore
 */
import app from 'app';
import 'js/aw-togglebutton.directive';
import 'js/aw-listbox.directive';
import 'js/extended-tooltip.directive';
import 'js/aw-pic.directive';
import 'js/aw-chip.directive';
import 'js/aw-icon-button.directive';
import "js/enable-when.directive";
import "js/visible-when.directive";
import uwPropertySvc from 'js/uwPropertyService';
import eventBus from 'js/eventBus';
import angular from 'angular';
/**
 * A custom element representing a element to be used for zooming in and out of pages.
 * @typedef "mom-namerev-asc-card"
 * @property {Expression} prop Property to bind with the control
 * @implements {Element}
 * @example
 * <mom-namerev-asc-card name="Id"></mom-namerev-asc-card>
 */
app.directive( 'momNamerevAscCard', [
    'viewModelService',
    function( viewModelSvc ) {
        return {
            restrict: 'E',
            scope: {
                name: '@?',
                prop: '='
            },
            controller: [ '$scope', function( $scope ) {
                // Set up Incoming Decl View Model
                $scope.revTextFlag = false;
                $scope.isDefaultText = true;
                $scope.isEntityVisible = false;
                $scope.isRevisionInputVisible = false;
                $scope.nameText = "Drag or Select an Item";
                var declViewModel = viewModelSvc.getViewModel( $scope, true );
                viewModelSvc.bindConditionStates( declViewModel, $scope );
                $scope.conditions = declViewModel.getConditionStates();

                var numberValues = uwPropertySvc.createViewModelProperty( "revisionNumberValues", "", "OBJECT", [], "" );
                uwPropertySvc.setHasLov( numberValues, true );
                uwPropertySvc.setIsSelectOnly( numberValues, true );
                uwPropertySvc.setIsEnabled( numberValues, true );

                var numberList = uwPropertySvc.createViewModelProperty( "revisionNumberObject", "", "OBJECT", [], "" );
                uwPropertySvc.setHasLov( numberList, true );
                uwPropertySvc.setIsSelectOnly( numberList, true );
                numberList.propApi = {};
                uwPropertySvc.setEditState( numberList, true, true );
                $scope.revisionNumberObject = numberList;
                $scope.revisionNumberValues = numberValues;

                var nameValues = uwPropertySvc.createViewModelProperty( "revisionNameValues", "", "OBJECT", [], "" );
                uwPropertySvc.setHasLov( nameValues, true );
                uwPropertySvc.setIsSelectOnly( nameValues, true );
                uwPropertySvc.setIsEnabled( nameValues, true );

                var nameList = uwPropertySvc.createViewModelProperty( "revisionNameObject", "", "OBJECT", [], "" );
                uwPropertySvc.setHasLov( nameList, true );
                uwPropertySvc.setIsSelectOnly( nameList, true );
                nameList.propApi = {};
                uwPropertySvc.setEditState( nameList, true, true );
                $scope.revisionNameObject = nameList;
                $scope.revisionNameValues = nameValues;

                //Setting up the Variables for the Card from ViewModel
                $scope.isRORToggleDisabled = false;
                $scope.imageAsc = {
                    dbValue: "assets/image/typeWorkflow48.svg"
                };
                var rorToggleVmo = uwPropertySvc.createViewModelProperty( "rorToggle", "", "BOOLEAN", false, [] );
                rorToggleVmo.isEditable = false;
                $scope.rorToggle = rorToggleVmo;
                $scope.rorSelectedChip = {
                    chipType: "STATIC",
                    labelDisplayName: "ROR"
                };
                $scope.toggleEntityDropdown = function() {
                    $scope.isEntityVisible = !$scope.isEntityVisible;
                };
                $scope.toggleRevisionDropdown = function( prop ) {
                    $scope.revTextFlag = !$scope.revTextFlag;
                    $scope.isRevisionInputVisible = !$scope.revTextFlag;
                    setNumberListBoxEntities( $scope, $scope.revCardInputs.revisionEntities, prop );
                };
                $scope.setListBoxValues = function() {
                    setNameListBoxEntities( $scope, $scope.revCardInputs.revisionEntities );
                };
                $scope.binCommand = {
                    action: "",
                    iconName: "cmdTrash",
                    tooltip: "Delete"
                };
                $scope.questionMarkCommand = {
                    dbValue: "assets/image/cmdHelp16.svg",
                    extendedTooltip: {
                        view: ""
                    }
                };
                //Getting the config from the DeclView Model
                $scope.revCardInputs = declViewModel.revCardInputs !== undefined ? declViewModel.revCardInputs.find( x => x.propertyName === $scope.name ) : undefined;
                if( $scope.revCardInputs !== undefined ) {
                    $scope.binCommand.action = $scope.revCardInputs.action;
                    $scope.labelText = $scope.revCardInputs.labelText;
                    $scope.showLabel = $scope.revCardInputs.showLabel;
                    $scope.isCardRequired = $scope.revCardInputs.cardRequired !== undefined ? $scope.revCardInputs.cardRequired : false;
                    $scope.questionMarkCommand.toolTipView = $scope.revCardInputs.toolTipView;
                    $scope.enableCustomToolTip = $scope.revCardInputs.enableCustomToolTip;
                    $scope.setListBoxValues();
                }
                if( $scope.prop !== undefined ) {
                    $scope.prop.uiValue = $scope.momNameInputValue;
                    $scope.prop.dbValue = $scope.revisionNameObject.dbValue;
                }
            } ],
            replace: true,
            templateUrl: app.getBaseUrlPath() + '/html/mom-namerev-asc-card.directive.html',
            link: function( scope, element ) {
                scope.setFieldsOnTab = function( event, keyCode ) {
                    event.cancelable = true;
                    event.stopPropagation();
                    event.preventDefault();
                    if( keyCode === 9 && event.target.id === "mom-revCard-asc" ) {
                        scope.toggleEntityDropdown();
                    }
                };
                eventBus.subscribe( scope.name + "_identify_mom_card", function() {
                    var angElement = angular.element( element[ 0 ].children[ 1 ] );
                    angElement.removeClass( "mom-asc-animation-class" );
                    //offset before re-running animation
                    angElement.offset();
                    angElement.addClass( "mom-asc-animation-class" );
                } );
                eventBus.subscribe( scope.name + "_dropEvent_mom_card", function( data ) {
                    scope.$apply( function() {
                        scope.revCardInputs = data.entity;
                        scope.setListBoxValues();
                        scope.revisionNumberObject.dbValue = data.entity.revisionEntities[ 0 ].revision;

                    } );
                } );
            }
        };
    }
] );
/*eslint-disable*/
function setNameListBoxEntities( $scope, data ) {
    data.forEach( element => {
        var dbValueToAdd = {};
        var child = {};
        if( element.isROR ) {
            dbValueToAdd = {
                propDisplayValue: element.name,
                propDisplayDescription: "",
                dispValue: element.name,
                propInternalValue: "",
                hasChildren: true,
                children: [],
                lovType: "OBJECT",
                propHasValidValues: true,
                iconName: "BIGROR",
                sel: false
            };
            child = {
                propInternalValue: element.revision,
                propDisplayValue: element.name + ": " + element.revision,
                dispValue: element.name + ": " + element.revision,
                parentName: element.name,
                propDisplayDescription: "",
                iconName: "Check"
            };
            if( $scope.revisionNameValues.dbValue.some( x => x.propDisplayValue === dbValueToAdd.propDisplayValue ) ) {
                var parent = $scope.revisionNameValues.dbValue.filter( x => x.propDisplayValue === dbValueToAdd.propDisplayValue )[ 0 ];
                if( !parent.children.some( x => x.propInternalValue === child.propInternalValue ) ) {
                    parent.children.push( child );
                }
            } else {
                dbValueToAdd.children.push( child );
                $scope.revisionNameValues.dbValue.push( dbValueToAdd );
            }
        } else {
            child = {
                propInternalValue: element.revision,
                propDisplayValue: element.name + ": " + element.revision,
                dispValue: element.name + ": " + element.revision,
                propDisplayDescription: ""
            };
            dbValueToAdd = $scope.revisionNameValues.dbValue.filter( x => x.propDisplayValue === element.name )[ 0 ];
            dbValueToAdd.children.push( child );
        }
    } );

    uwPropertySvc.setValue( $scope.revisionNameObject, $scope.revisionNameValues.dbValue[ 0 ].propInternalValue );
    uwPropertySvc.updateDisplayValues( $scope.revisionNameObject, [ $scope.revisionNameValues.dbValue[ 0 ].propInternalValue ] );

    $scope.$watch( 'revisionNameObject.dbValue', function() {
        if( $scope.revisionNameObject.dbValue !== "" ) {
            $scope.revTextFlag = true;
            $scope.isRevisionInputVisible = false;
            var name = $scope.revisionNameObject.uiValue.split( ':' )[ 0 ];
            $scope.nameText = name;
            $scope.isEntityVisible = false;
            $scope.isDefaultText = false;
            if( $scope.prop !== undefined ) {
                $scope.prop.uiValue = $scope.nameText;
            }
        }
    } );
    $scope.$watch( 'revisionNumberObject.dbValue', function() {
        if( $scope.revisionNumberObject.dbValue !== "" && $scope.revisionNumberObject.dbValue.length !== 0 && $scope.revisionNameObject.dbValue !== $scope.revisionNumberObject.dbValue ) {
            // Flags will be taken calre by name watch.
            // Update the Name Object
            var allItems = [];
            $scope.revisionNameValues.dbValue.forEach( element => {
                allItems = [ ...allItems, ...element.children ];
            } );
            var selectedEntry = allItems.find( x => x.propInternalValue === $scope.revisionNumberObject.dbValue );
            $scope.revisionNameObject.dbValue = selectedEntry.propInternalValue;
            $scope.revisionNameObject.uiValue = selectedEntry.dispValue;
            if( $scope.revisionNameObject.selectedLovEntries.length > 0 ) {
                $scope.revisionNameObject.selectedLovEntries[ 0 ].propDisplayValue = selectedEntry.dispValue;
                $scope.revisionNameObject.selectedLovEntries[ 0 ].propInternalValue = selectedEntry.propInternalValue;
            }
            $scope.revisionNumberObject.dbValue = selectedEntry.propInternalValue;
            if( $scope.prop !== undefined ) {
                $scope.prop.dbValue = $scope.revisionNumberObject.dbValue;
            }

        }
    } );
}

function setNumberListBoxEntities( $scope, data, prop ) {
    var selectedItem = prop.selectedLovEntries[ 0 ];
    var parentName = selectedItem.propDisplayValue.split( ':' )[ 0 ];
    data.forEach( element => {
        if( element.name === parentName ) {
            var dbValueToAdd = {
                propDisplayValue: element.revision,
                propDisplayDescription: "",
                dispValue: element.revision,
                propInternalValue: element.revision
            };
            if( element.isROR ) {
                dbValueToAdd.iconName = "Check";
            }
            if( !$scope.revisionNumberValues.dbValue.some( x => x.propInternalValue === dbValueToAdd.propInternalValue ) ) {
                $scope.revisionNumberValues.dbValue.push( dbValueToAdd );
            }
        }
    } );
    uwPropertySvc.setValue( $scope.revisionNumberObject, selectedItem.propInternalValue );
    uwPropertySvc.updateDisplayValues( $scope.revisionNumberObject, [ selectedItem.propInternalValue ] );
}
