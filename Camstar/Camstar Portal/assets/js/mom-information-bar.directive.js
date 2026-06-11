// Copyright 2020 Siemens AG
/*global
 define
 */
/* eslint-disable valid-jsdoc */
/**
 * A module exposing a custom element representing an information bar.
 * @module "js/mom-information-bar.directive"
 */
import app from 'app';
import 'js/aw-command-bar.directive';
import 'js/viewModelService';

'use strict';

/**
 * A custom element representing an information bar to show a message of type note, help, general, success, warning, and errors.
 * @typedef "mom-information-bar"
 * @property {String} [icon=""] The name of a command or indicator icon.
 * @property {String} [label=""] An optional title for the information bar.
 * @property {String} [type="general"] The type of information bar (one of the following: note, help, general, success, warning, error).
 * @property {String} [link-text=""] The text of the optional link to add at the end of the information bar.
 * @property {String} [link-action=""] The name of an action to execute when the optional link is clicked.
 * @property {String} [mode="standard"] The information bar mode (**standard** or **banner**).
 * @implements {Element}
 * @example
 * <mom-information-bar
 *      icon="indicatorInformationGeneral"
 *      type="general"
 *      linkAction="showExtraInfo"
 *      linkText="More information..."
 *      label="Markdown Support">
 *  You can use the Markdown markup language in the field below.
 * </mom-information-bar>
 */
app.directive( 'momInformationBar', [ 'viewModelService', 'iconService', '$sce', function( vmSvc, iconSvc, $sce ) {
    return {
        restrict: 'E',
        scope: {
            icon: '@',
            type: '@',
            linkText: '@',
            linkAction: '@',
            label: '@',
            mode: '@'
        },
        transclude: true,
        controller: [ '$scope', function( $scope ) {
            $scope.type = $scope.type || 'general';
            $scope.mode = $scope.mode || 'standard';
            if( !$scope.icon ) {
                $scope.iconSvg = null;
            } else {
                if( $scope.icon.match( /^cmd/ ) || !$scope.icon.match( /^indicator/ ) ) {
                    $scope.iconSvg = $sce.trustAsHtml( iconSvc.getCmdIcon( $scope.icon.replace( /^cmd/, '' ) ) );
                    $scope.iconClass = 'mom-information-bar-command-icon';
                } else {
                    $scope.iconSvg = $sce.trustAsHtml( iconSvc.getIcon( $scope.icon ) );
                    $scope.iconClass = 'mom-information-bar-icon';
                }
            }
            $scope.execute = function( action ) {
                if( action ) {
                    let declViewModel = vmSvc.getViewModel( $scope, true );
                    vmSvc.executeCommand( declViewModel, action, $scope );
                }
            };
        } ],
        templateUrl: app.getBaseUrlPath() + '/html/mom-information-bar.html'
    };
} ] );
