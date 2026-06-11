// Copyright 2020 Siemens AG
/*global
 define
 */

/**
 * @module js/mom-header-content.directive
 */
import app from 'app';
import eventBus from 'js/eventBus';
import 'js/aw-include.directive';
import 'js/aw-command-bar.directive';
import 'js/aw-row.directive';
import 'js/aw-column.directive';
import 'js/appCtxService';

'use strict';

app.directive( 'momHeaderContent', [ 'appCtxService', function( appCtxService ) {
    return {

        restrict: 'E',
        scope: {},
        controller: [ '$state', '$scope', function( $state, $scope ) {
            $scope.viewBase = $state.current.data && $state.current.data.headerCustomContent || appCtxService.ctx.headerCustomContent;

            let headerCustomContentSubscription = eventBus.subscribe( "appCtx.*", function( data ) {
                if( data.name === 'headerCustomContent' ) {
                    $scope.viewBase = data.value;
                }
            } );

            $scope.$on( '$destroy', function() {
                eventBus.unsubscribe( headerCustomContentSubscription );
            } );
        } ],

        templateUrl: app.getBaseUrlPath() + '/html/mom-header-content.directive.html'
    };
} ] );
