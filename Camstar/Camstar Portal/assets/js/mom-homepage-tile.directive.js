// Copyright 2020 Siemens AG
/*global
 define
 */
/* eslint-disable valid-jsdoc */
/**
 * A module exposing a custom element for a tile to be used in second-level home pages.
 * @module "js/mom-homepage-tile.directive"
 */
import app from 'app';
import 'js/viewModelService';
import 'js/iconService';

'use strict';

/**
 * A custom element representing a tile to be used in second-level home pages.
 * @typedef "mom-homepage-tile"
 * @property {Expression} icon The name of an icon display in the tile.
 * @property {String} action The action to perform when the tile is clicked.
 * @property {Expression} header The title of the tile.
 * @implements {Element}
 * @example
 * <mom-homepage-tile
 *      icon="'typeLogBook'"
 *      action="goToProject1Page"
 *      header="'Project #1'">
 * </mom-homepage-tile>
 */
app.directive( 'momHomepageTile', [ 'viewModelService', 'iconService', '$sce', function( vmSvc, iconSvc, $sce ) {
    return {
        restrict: 'E',
        scope: {
            icon: '=',
            action: '@',
            header: '='
        },
        transclude: true,
        controller: [ '$scope', function( $scope ) {
            if( !$scope.icon ) {
                $scope.iconSvg = null;
            } else if( $scope.icon.match( /^type/ ) ) {
                $scope.iconSvg = $sce.trustAsHtml( iconSvc.getTypeIcon( $scope.icon.replace( /^type/, '' ) ) );
            } else {
                $scope.iconSvg = $sce.trustAsHtml( iconSvc.getIcon( $scope.icon ) );
            }
            $scope.iconSvg = $scope.iconSvg || $sce.trustAsHtml( iconSvc.getTypeIcon( 'MissingImage' ) );
            $scope.execute = function( action ) {
                let declViewModel = vmSvc.getViewModel( $scope, true );
                vmSvc.executeCommand( declViewModel, action, $scope );
            };
        } ],
        templateUrl: app.getBaseUrlPath() + '/html/mom-homepage-tile.html'
    };
} ] );
