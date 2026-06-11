// Copyright 2020 Siemens AG

/*global
 define
 */
/* eslint-disable valid-jsdoc */
/**
 * A module exposing a custom element used as a touch friendly input.
 * @module "js/mom-zoom-control.directive"
 * @ignore
 */
import app from 'app';
import 'js/aw-property-label.directive';
import 'js/aw-property-non-edit-val.directive';
import 'js/aw-numeric.directive';
import 'js/exist-when.directive';
import 'js/aw-icon-button.directive';
import _ from 'lodash';
import eventBus from 'js/eventBus';
import "js/enable-when.directive";

/**
 * A custom element representing a element to be used for zooming in and out of pages.
 * @typedef "mom-zoom-control"
 * @property {Expression} prop Property to bind with the control
 * @implements {Element}
 * @example
 * <mom-zoom-control prop="'data.prop'"></mom-zoom-control>
 */
app.directive( 'momZoomControl', [
    'viewModelService',
    function( viewModelSvc ) {
        return {
            restrict: 'E',
            scope: {
                prop: '=',
                commands: '=?'
            },
            controller: [ '$scope', function( $scope ) {
                var declViewModel = viewModelSvc.getViewModel( $scope, true );
                viewModelSvc.bindConditionStates( declViewModel, $scope );
                $scope.conditions = declViewModel.getConditionStates();
                // Set the Scope
                $scope.zoomConfig = declViewModel.momZoomConfigs !== undefined ? declViewModel.momZoomConfigs.find( x => x.propertyName === $scope.prop.propertyName ) : undefined;
                var defaultZoomLevel = $scope.zoomConfig !== undefined ? $scope.zoomConfig.defaultZoomLevel : "100%";
                var zoomTextElements = document.querySelectorAll( "#momZoomText" );
                zoomTextElements.forEach( zoomText => {
                    zoomText.value = defaultZoomLevel;
                } );

                $scope.leftMinusCommands = [ {
                    iconName: "miscDecrease_uxRefresh",
                    tooltip: "Zoom Out",
                    isDefault: true
                } ];
                $scope.rightPlusCommands = [ {
                    iconName: "miscIncrease_uxRefresh",
                    tooltip: "Zoom In",
                    isDefault: true
                } ];

                // Setting the default Zoom
                var mainDivName = $scope.zoomConfig === undefined ? "main-view" : $scope.zoomConfig.mainContainer;
                var mainDiv = document.getElementById( mainDivName );
                mainDiv.style.zoom = defaultZoomLevel;
                $scope.ResetMag = function() {
                    var mainDivName = $scope.zoomConfig === undefined ? "main-view" : $scope.zoomConfig.mainContainer;
                    var mainDiv = document.getElementById( mainDivName );
                    zoomTextElements.forEach( zoomText => {
                        zoomText.value = defaultZoomLevel;
                    } );
                    mainDiv.style.zoom = defaultZoomLevel;
                };
            } ],
            replace: true,
            templateUrl: app.getBaseUrlPath() + '/html/mom-zoom-control.directive.html',
            link: function( scope, element ) {
                element.find( ':first' ).on( 'click', function( event ) {
                    var sourceButton = getButtonSource( event.target );
                    if( sourceButton.isValid ) {
                        commandDefaultBehaviour( scope, sourceButton.button );
                        eventBus.publish( 'momZoomControl.command.clicked', {
                            buttonClicked: sourceButton.button,
                            isDefaultBehavior: true
                        } );
                    }
                } );
            }
        };
    }
] );
/* eslint-disable*/
function commandDefaultBehaviour( scope, operator ) {
    var mainDivName = scope.zoomConfig === undefined ? "main-view" : scope.zoomConfig.mainContainer;
    var mainDiv = document.getElementById( mainDivName );
    var zoomLevel = isNaN( parseInt( mainDiv.style.zoom, 10 ) ) ? 100 : parseInt( mainDiv.style.zoom, 10 );
    var zoomLevelsArray = [ 90, 100, 110, 125, 150, 200, 250, 350, 500 ]
    var zoomLevelsIndex = zoomLevelsArray.indexOf( zoomLevel );
    var zoomTextElements = document.querySelectorAll( "#momZoomText" );
    if( operator === "leftMinusCommand" ) {
        mainDiv.style.zoom = zoomLevelsIndex - 1 < 0 ? "90%" : zoomLevelsArray[ zoomLevelsIndex - 1 ] + "%";
        zoomTextElements.forEach( zoomText => {
            zoomText.value = zoomLevelsIndex - 1 < 0 ? "90%" : zoomLevelsArray[ zoomLevelsIndex - 1 ] + "%";
        } );
    } else if( operator === "rightPlusCommand" ) {
        mainDiv.style.zoom = zoomLevelsIndex + 1 > 8 ? "500%" : zoomLevelsArray[ zoomLevelsIndex + 1 ] + "%";
        zoomTextElements.forEach( zoomText => {
            zoomText.value = zoomLevelsIndex + 1 > 8 ? "500%" : zoomLevelsArray[ zoomLevelsIndex + 1 ] + "%";
        } );
    }
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
