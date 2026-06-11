// Copyright 2020 Siemens AG

/*global
 define
 */
/* eslint-disable valid-jsdoc */
/**
 * A module exposing a custom element used as a touch friendly input.
 * @module "js/mom-namerev-card.directive"
 * @ignore
 */
import app from 'app';
import 'js/aw-togglebutton.directive';
import 'js/aw-pic.directive';
import 'js/aw-chip.directive';
import 'js/aw-textbox.directive';
import 'js/aw-icon-button.directive';
import "js/enable-when.directive";
import uwPropertySvc from 'js/uwPropertyService';
import eventBus from 'js/eventBus';
import angular from 'angular';

/**
 * A custom element representing a element to be used for zooming in and out of pages.
 * @typedef "mom-namerev-card"
 * @property {Expression} prop Property to bind with the control
 * @implements {Element}
 * @example
 * <mom-namerev-card name="Id"></mom-namerev-card>
 */
app.directive( 'momNamerevCard', [
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
                var declViewModel = viewModelSvc.getViewModel( $scope, true );
                viewModelSvc.bindConditionStates( declViewModel, $scope );
                $scope.conditions = declViewModel.getConditionStates();

                //Setting up the Variables for the Card from ViewModel
                $scope.isRORToggleDisabled = false;
                $scope.image = {
                    dbValue: "assets/image/typeWorkflow48.svg"
                };
                var rorToggleVmo = uwPropertySvc.createViewModelProperty( "rorToggle", "", "BOOLEAN", false, [] );
                rorToggleVmo.isEditable = false;
                $scope.rorToggle = rorToggleVmo;
                $scope.rorSelectedChip = {
                    chipType: "STATIC",
                    iconId: "indicatorCheckmarkGreen",
                    labelDisplayName: "Base",
                    labelInternalName: "base",
                    showLabel: false,
                    showIcon: true
                };
                //Getting the config from the DeclView Model
                $scope.revCardInputs = declViewModel.revCardInputs !== undefined ? declViewModel.revCardInputs.find( x => x.propertyName === $scope.name ) : undefined;
                if( $scope.revCardInputs !== undefined ) {
                    $scope.hasNoRevision = $scope.revCardInputs.hasNoRevision !== undefined ? $scope.revCardInputs.hasNoRevision : false;
                    $scope.id = $scope.revCardInputs.id;
                    $scope.isLocked = $scope.revCardInputs.isLocked;
                    $scope.revCardInputs.currentRORValue = $scope.revCardInputs.currentRORValue === undefined ? "" : $scope.revCardInputs.currentRORValue;
                    $scope.momNameInputValue = $scope.revCardInputs.name;
                    $scope.momRevisionInputValue = $scope.revCardInputs.revision;
                    $scope.labelText = $scope.revCardInputs.labelText;
                    $scope.showLabel = $scope.revCardInputs.showLabel;
                    $scope.isCardRequired = $scope.revCardInputs.cardRequired !== undefined ? $scope.revCardInputs.cardRequired : false;
                }
                $scope.isRORToggleDisabled = $scope.momRevisionInputValue !== "" && $scope.momRevisionInputValue !== undefined;
                $scope.isRORRev = ( $scope.momRevisionInputValue === $scope.revCardInputs.currentRORValue ) || $scope.revCardInputs.isNewInstance;
                $scope.enableRORToggle = function() {
                    $scope.isRORToggleDisabled = $scope.momRevisionInputValue !== "";
                };
                $scope.$watch( 'momNameInputValue', function() {
                $scope.prop.uiValue = $scope.momNameInputValue;
                });
                $scope.$watch( 'momRevisionInputValue', function() {
                $scope.prop.dbValue = $scope.momRevisionInputValue;
                });
            } ],
            replace: true,
            templateUrl: app.getBaseUrlPath() + '/html/mom-namerev-card.directive.html',
            link: function( scope, element ) {
                eventBus.subscribe( scope.name + "_identify_mom_card", function() {
                    var angElement = angular.element( element[ 0 ].children[ 1 ] );
                    angElement.removeClass( "mom-asc-animation-class" );
                    //offset before re-running animation
                    angElement.offset();
                    angElement.addClass( "mom-asc-animation-class" );
                } );
            }
        };
    }
] );
